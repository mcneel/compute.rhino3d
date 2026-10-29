using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace rhino.compute
{
    /// <summary>
    /// POST /mcp: the RhinoAI MCP server (MCP Streamable HTTP, JSON responses only), served by
    /// the ordinary compute.geometry pool.
    ///
    /// <para>Every pool child starts the RhinoAI plug-in's loopback JSON-RPC listener on a port
    /// derived from its own (<see cref="ListenerPortFor"/>); that listener is bound to the child's
    /// startup document. <c>initialize</c> without an <c>Mcp-Session-Id</c> takes a child the way
    /// /grasshopper does, asks that listener for a fresh headless document
    /// (<c>_router_spawn_listener</c>), and maps a random id to the child and the new document's
    /// listener port, returned in the response header. Later requests carrying the id go to that
    /// document only. DELETE /mcp closes a session; idle ones expire. Requests without a session go
    /// to a round-robin child's startup document, as they did when the listener was reached
    /// directly.</para>
    ///
    /// <para>MCP requests count as pool activity, so the children's idle span sees them. Children
    /// still exit when the whole pool is idle for the span, or on /recycle-children, and their
    /// sessions go with them: the client gets 404 and starts over with initialize.</para>
    /// </summary>
    public static class McpProxyModule
    {
        /// <summary>Shared with the RhinoAI plug-in: the variable it reads to start its listener.</summary>
        public const string ListenerPortEnvVar = "RHINO_MCP_AUTOSTART_PORT";
        public const string SessionHeader = "Mcp-Session-Id";

        // Pool children are spawned from this port upwards (see ComputeChildren.FindFreePort).
        const int FirstChildPort = 6001;

        // Each child gets a block of listener ports: its startup document's listener at the
        // block's first port, and the plug-in puts session documents on the following ones
        // (highest in use + 1). The block keeps a child's session listeners from taking the
        // next child's base port.
        const int PortsPerChild = 100;

        // The plug-in's router-private control tools (spawn/close listener, quit). They are the
        // proxy's control channel and must not be reachable by agents through /mcp.
        const string RouterToolPrefix = "_router_";

        // A child's HTTP port can open before Rhino has finished loading plug-ins; the listener
        // follows. 80 s on an arm64 VM.
        static readonly TimeSpan ListenerStartTimeout = TimeSpan.FromSeconds(180);
        static readonly TimeSpan ControlCallTimeout = TimeSpan.FromSeconds(30);
        static readonly TimeSpan HousekeepingInterval = TimeSpan.FromSeconds(30);

        sealed class McpSession
        {
            public string Id;
            public int ChildPort;      // the compute.geometry child hosting the document...
            public int ChildPid;       // ...and its process id: a replacement child reuses the port
            public int ListenerPort;   // the document's listener in that child
            public DateTime LastSeen;
        }

        static HttpClient client;
        static readonly ConcurrentDictionary<string, McpSession> sessions = new ConcurrentDictionary<string, McpSession>(StringComparer.Ordinal);
        // Child process id -> that child's startup-document listener, once it has answered a ping.
        static readonly ConcurrentDictionary<int, int> baseListeners = new ConcurrentDictionary<int, int>();
        // The plug-in's listener table is not thread-safe on a headless host: one control call at a time.
        static readonly SemaphoreSlim controlLock = new SemaphoreSlim(1, 1);
        static System.Threading.Timer housekeeping;
        static int housekeepingBusy;

        /// <summary>
        /// The listener port handed to the child on <paramref name="childPort"/>: the configured
        /// port for the first child, then one block of <see cref="PortsPerChild"/> ports per
        /// child. 0 when the block would leave the port range (a /launch-child?port=N far above
        /// the usual 6001-6256), in which case that child gets no listener.
        /// </summary>
        public static int ListenerPortFor(int childPort)
        {
            int port = Config.McpPort + (childPort - FirstChildPort) * PortsPerChild;
            return port >= 1 && port + PortsPerChild - 1 <= 65535 ? port : 0;
        }

        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/mcp", HandlePost);
            app.MapDelete("/mcp", HandleDelete);
            app.MapGet("/mcp", HandleGet);

            if (!Config.McpEnabled)
                return;

            client = new HttpClient(new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(60)
            });
            client.DefaultRequestHeaders.Add("User-Agent", "compute.rhino3d-mcp-proxy/1.0.0");
            client.Timeout = TimeSpan.FromSeconds(Config.ReverseProxyRequestTimeout);

            housekeeping = new System.Threading.Timer(_ => Housekeep(), null, HousekeepingInterval, HousekeepingInterval);
        }

        // ---------------------------------------------------------------- endpoints

        // No SSE stream is offered, which the spec says to answer with 405.
        static async Task HandleGet(HttpRequest req, HttpResponse res)
        {
            res.StatusCode = StatusCodes.Status405MethodNotAllowed;
            res.Headers["Allow"] = "POST, DELETE";
            await res.WriteAsJsonAsync(new
            {
                error = "The MCP server has no server-to-client stream. POST JSON-RPC messages to /mcp; responses are JSON."
            });
        }

        static async Task HandleDelete(HttpRequest req, HttpResponse res)
        {
            if (!Config.McpEnabled)
            {
                await WriteDisabledAsync(res);
                return;
            }
            string sessionId = GetSessionId(req);
            if (sessionId == null)
            {
                res.StatusCode = StatusCodes.Status400BadRequest;
                await res.WriteAsJsonAsync(new { error = $"DELETE /mcp needs the {SessionHeader} header of the session to close." });
                return;
            }
            if (!sessions.TryRemove(sessionId, out var session))
            {
                res.StatusCode = StatusCodes.Status404NotFound;
                await res.WriteAsJsonAsync(new { error = "Session not found." });
                return;
            }
            await CloseSessionAsync(session, "closed by client", req.HttpContext.RequestAborted);
            res.StatusCode = StatusCodes.Status200OK;
            await res.WriteAsJsonAsync(new { closed = sessionId });
        }

        static async Task HandlePost(HttpRequest req, HttpResponse res)
        {
            if (!Config.McpEnabled)
            {
                await WriteDisabledAsync(res);
                return;
            }

            var ct = req.HttpContext.RequestAborted;

            // Read in full first, so the timeout covers the child's work and not the client's upload.
            var bodyStream = new MemoryStream((int)Math.Clamp(req.ContentLength ?? 0, 0, int.MaxValue));
            await req.Body.CopyToAsync(bodyStream, ct);
            byte[] body = bodyStream.ToArray();

            string method = null;
            string toolName = null;
            JsonNode requestId = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("method", out var m) && m.ValueKind == JsonValueKind.String)
                        method = m.GetString();
                    if (root.TryGetProperty("id", out var id))
                        requestId = JsonNode.Parse(id.GetRawText());
                    if (method == "tools/call"
                        && root.TryGetProperty("params", out var p) && p.ValueKind == JsonValueKind.Object
                        && p.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                        toolName = n.GetString();
                }
            }
            catch (JsonException ex)
            {
                await WriteRpcErrorAsync(res, StatusCodes.Status400BadRequest, null, -32700, $"Parse error: {ex.Message}");
                return;
            }

            if (toolName != null && toolName.StartsWith(RouterToolPrefix, StringComparison.Ordinal))
            {
                await WriteRpcErrorAsync(res, StatusCodes.Status200OK, requestId, -32601, $"Tool {toolName} is not available through /mcp.");
                return;
            }

            bool isInitialize = method == "initialize";
            string sessionId = GetSessionId(req);
            McpSession session = null;
            bool newSession = false;
            ComputeChildren.ChildLease lease = null;
            int targetPort;

            try
            {
                if (sessionId != null)
                {
                    // 404 is the spec's signal to start over with a new initialize.
                    if (!sessions.TryGetValue(sessionId, out session) || !ComputeChildren.TryLeaseChild(session.ChildPort, session.ChildPid, out lease))
                    {
                        if (session != null && sessions.TryRemove(sessionId, out _))
                            Log.Information("MCP session {Session} dropped: its compute.geometry child on port {Port} is gone", Abbrev(sessionId), session.ChildPort);
                        await WriteRpcErrorAsync(res, StatusCodes.Status404NotFound, requestId, -32001,
                            "Session not found or expired. Start a new session with initialize and no " + SessionHeader + " header.");
                        return;
                    }
                    session.LastSeen = DateTime.Now;
                    targetPort = session.ListenerPort;
                }
                else
                {
                    // Same path as every proxied endpoint: a round-robin child, started if need be.
                    await ReverseProxyModule.AwaitInitTask();
                    try
                    {
                        lease = ComputeChildren.AcquireChild();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "No compute.geometry child for /mcp");
                        await WriteRpcErrorAsync(res, StatusCodes.Status503ServiceUnavailable, requestId, -32000,
                            "No compute.geometry child is available. Check the rhino.compute log.");
                        return;
                    }

                    int childPid = ComputeChildren.ChildProcessId(lease.Port);
                    int basePort = childPid == 0 ? 0 : await WaitForBaseListenerAsync(lease.Port, childPid, ct);
                    if (basePort == 0)
                    {
                        await WriteRpcErrorAsync(res, StatusCodes.Status503ServiceUnavailable, requestId, -32000,
                            $"compute.geometry on port {lease.Port} has no MCP listener. Is the RhinoAI plug-in installed, and is port {ListenerPortFor(lease.Port)} free? Check the rhino.compute log.");
                        return;
                    }

                    if (isInitialize)
                    {
                        if (sessions.Count >= Config.McpMaxSessions)
                        {
                            await WriteRpcErrorAsync(res, StatusCodes.Status503ServiceUnavailable, requestId, -32000,
                                $"Session limit reached ({Config.McpMaxSessions}). Close a session with DELETE /mcp or wait for idle sessions to expire.");
                            return;
                        }
                        int childPort = lease.Port;
                        if (sessions.Values.Count(x => x.ChildPort == childPort) >= PortsPerChild - 1)
                        {
                            await WriteRpcErrorAsync(res, StatusCodes.Status503ServiceUnavailable, requestId, -32000,
                                $"compute.geometry on port {childPort} already hosts {PortsPerChild - 1} sessions. Close a session with DELETE /mcp or retry.");
                            return;
                        }
                        int port;
                        try
                        {
                            port = await SpawnListenerAsync(basePort, ct);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            Log.Error(ex, "Opening an MCP session document in compute.geometry on port {Port} failed", lease.Port);
                            await WriteRpcErrorAsync(res, StatusCodes.Status503ServiceUnavailable, requestId, -32000,
                                $"Could not open a document for the session: {ex.Message}");
                            return;
                        }
                        session = new McpSession
                        {
                            Id = NewSessionId(),
                            ChildPort = lease.Port,
                            ChildPid = childPid,
                            ListenerPort = port,
                            LastSeen = DateTime.Now,
                        };
                        sessions[session.Id] = session;
                        newSession = true;
                        targetPort = port;
                        Log.Information("MCP session {Session} opened in compute.geometry on port {ChildPort}, listener port {Port} ({Count} open)",
                            Abbrev(session.Id), lease.Port, port, sessions.Count);
                    }
                    else
                    {
                        targetPort = basePort;
                    }
                }

                // Counts as pool activity, like any proxied request, so the idle span sees it.
                ComputeChildren.UpdateLastCall();

                using var upstream = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{targetPort}/");
                var content = new ByteArrayContent(body);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                upstream.Content = content;
                if (req.Headers.TryGetValue("Accept", out var accept))
                    upstream.Headers.TryAddWithoutValidation("Accept", accept.ToArray());

                using var proxyResponse = await client.SendAsync(upstream, ct);
                byte[] payload = await proxyResponse.Content.ReadAsByteArrayAsync(ct);
                ComputeChildren.UpdateLastCall();

                if (method == "tools/list" && proxyResponse.IsSuccessStatusCode)
                    payload = StripRouterTools(payload);

                res.StatusCode = (int)proxyResponse.StatusCode;
                if (session != null)
                    res.Headers[SessionHeader] = session.Id;
                if (res.StatusCode == StatusCodes.Status204NoContent)
                    return;
                if (proxyResponse.Content.Headers.ContentType != null)
                    res.ContentType = proxyResponse.Content.Headers.ContentType.ToString();
                await res.Body.WriteAsync(payload, ct);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                Log.Warning("MCP request {Method} timed out after {Timeout}s", method, Config.ReverseProxyRequestTimeout);
                await WriteRpcErrorAsync(res, StatusCodes.Status504GatewayTimeout, requestId, -32000,
                    "Gateway timeout: compute.geometry did not respond in time.");
            }
            catch (HttpRequestException ex)
            {
                Log.Warning("MCP request {Method} to listener port {Port} failed: {Message}", method, session?.ListenerPort ?? 0, ex.Message);
                if (newSession)
                    sessions.TryRemove(session.Id, out _);
                if (lease != null)
                    baseListeners.TryRemove(ComputeChildren.ChildProcessId(lease.Port), out _);
                await WriteRpcErrorAsync(res, StatusCodes.Status502BadGateway, requestId, -32000,
                    $"compute.geometry did not answer: {ex.Message}");
            }
            finally
            {
                lease?.Dispose();
            }
        }

        // ---------------------------------------------------------------- listeners

        // The child's startup-document listener, waiting for it after a fresh spawn.
        static async Task<int> WaitForBaseListenerAsync(int childPort, int childPid, CancellationToken ct)
        {
            if (baseListeners.TryGetValue(childPid, out int known))
                return known;

            int expected = ListenerPortFor(childPort);
            if (expected == 0)
            {
                Log.Warning("compute.geometry on port {Port} has no MCP listener port in range", childPort);
                return 0;
            }

            var deadline = DateTime.Now + ListenerStartTimeout;
            while (DateTime.Now < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (!ComputeChildren.IsChildAlive(childPort, childPid))
                    return 0;
                if (await PingAsync(expected, ct))
                {
                    baseListeners[childPid] = expected;
                    return expected;
                }
                await Task.Delay(1000, ct);
            }
            Log.Error("No MCP listener on port {ListenerPort} for compute.geometry on port {Port} within {Timeout}s. Is the RhinoAI plug-in installed in this Rhino, and is the port free? (The plug-in moves to another port when it is taken.)",
                expected, childPort, (int)ListenerStartTimeout.TotalSeconds);
            return 0;
        }

        static async Task<bool> PingAsync(int port, CancellationToken ct)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                using var ping = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/")
                {
                    Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"rhino.compute-probe\",\"method\":\"ping\"}", Encoding.UTF8, "application/json")
                };
                using var response = await client.SendAsync(ping, cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
            {
                return false;
            }
        }

        // ---------------------------------------------------------------- sessions

        static async Task<int> SpawnListenerAsync(int basePort, CancellationToken ct)
        {
            await controlLock.WaitAsync(ct);
            try
            {
                string text = await CallControlToolAsync(basePort, "_router_spawn_listener", new JsonObject(), ct);
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("port", out var p) && p.TryGetInt32(out int port) && port > 0)
                    return port;
                throw new InvalidOperationException($"_router_spawn_listener returned no port: {text}");
            }
            finally
            {
                controlLock.Release();
            }
        }

        static async Task CloseSessionAsync(McpSession session, string reason, CancellationToken ct)
        {
            Log.Information("MCP session {Session} closed ({Reason}); {Count} open", Abbrev(session.Id), reason, sessions.Count);
            // The document dies with its child; only a live child needs telling.
            if (!baseListeners.TryGetValue(session.ChildPid, out int basePort) || !ComputeChildren.IsChildAlive(session.ChildPort, session.ChildPid))
                return;
            await controlLock.WaitAsync(ct);
            try
            {
                var args = new JsonObject { ["port"] = session.ListenerPort };
                await CallControlToolAsync(basePort, "_router_close_listener", args, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Warning("Closing the document of MCP session {Session} failed: {Message}", Abbrev(session.Id), ex.Message);
            }
            finally
            {
                controlLock.Release();
            }
        }

        // Expire idle sessions and forget those whose child has gone.
        static void Housekeep()
        {
            if (Interlocked.CompareExchange(ref housekeepingBusy, 1, 0) != 0)
                return;
            try
            {
                HousekeepAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Warning("MCP housekeeping failed: {Message}", ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref housekeepingBusy, 0);
            }
        }

        static async Task HousekeepAsync()
        {
            foreach (var entry in baseListeners.ToArray())
            {
                // Key is the child's process id; the listener port tells us which child port it had.
                int childPort = FirstChildPort + (entry.Value - Config.McpPort) / PortsPerChild;
                if (!ComputeChildren.IsChildAlive(childPort, entry.Key))
                    baseListeners.TryRemove(entry.Key, out _);
            }

            var now = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(Config.McpSessionTimeout);
            foreach (var session in sessions.Values.ToArray())
            {
                if (!ComputeChildren.IsChildAlive(session.ChildPort, session.ChildPid))
                {
                    if (sessions.TryRemove(session.Id, out _))
                        Log.Information("MCP session {Session} dropped: its compute.geometry child on port {Port} is gone", Abbrev(session.Id), session.ChildPort);
                    continue;
                }
                if (timeout > TimeSpan.Zero && now - session.LastSeen > timeout && sessions.TryRemove(session.Id, out _))
                    await CloseSessionAsync(session, $"idle for {(int)(now - session.LastSeen).TotalSeconds}s", CancellationToken.None);
            }
        }

        // tools/call on a child's startup-document listener; returns the text of the first content
        // block, which the plug-in's control tools fill with a JSON object.
        static async Task<string> CallControlToolAsync(int listenerPort, string tool, JsonObject arguments, CancellationToken ct)
        {
            var request = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = Guid.NewGuid().ToString("N"),
                ["method"] = "tools/call",
                ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments },
            };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(ControlCallTimeout);
            using var message = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{listenerPort}/")
            {
                Content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json")
            };
            using var response = await client.SendAsync(message, cts.Token);
            string body = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"{tool} returned HTTP {(int)response.StatusCode}: {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var error))
                throw new InvalidOperationException($"{tool} failed: {error.GetRawText()}");
            if (!root.TryGetProperty("result", out var result))
                throw new InvalidOperationException($"{tool} returned no result: {body}");
            if (result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
                throw new InvalidOperationException($"{tool} failed: {FirstText(result)}");
            return FirstText(result) ?? throw new InvalidOperationException($"{tool} returned no text content: {body}");
        }

        static string FirstText(JsonElement result)
        {
            if (!result.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return null;
            foreach (var block in content.EnumerateArray())
            {
                if (block.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    return text.GetString();
            }
            return null;
        }

        // tools/list from the plug-in includes its router-private control tools; agents must not
        // see them (they are refused on tools/call above).
        static byte[] StripRouterTools(byte[] payload)
        {
            try
            {
                var node = JsonNode.Parse(payload);
                if (node?["result"]?["tools"] is not JsonArray tools)
                    return payload;
                var hidden = tools
                    .Where(t => t?["name"]?.GetValue<string>() is string name && name.StartsWith(RouterToolPrefix, StringComparison.Ordinal))
                    .ToArray();
                if (hidden.Length == 0)
                    return payload;
                foreach (var t in hidden)
                    tools.Remove(t);
                return Encoding.UTF8.GetBytes(node.ToJsonString());
            }
            catch (JsonException)
            {
                return payload;
            }
        }

        // ---------------------------------------------------------------- helpers

        static string GetSessionId(HttpRequest req)
        {
            if (!req.Headers.TryGetValue(SessionHeader, out var value))
                return null;
            string id = value.ToString().Trim();
            return id.Length == 0 ? null : id;
        }

        static string NewSessionId()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        static string Abbrev(string sessionId) => sessionId.Length > 8 ? sessionId.Substring(0, 8) : sessionId;

        static async Task WriteDisabledAsync(HttpResponse res)
        {
            res.StatusCode = StatusCodes.Status404NotFound;
            await res.WriteAsJsonAsync(new
            {
                error = $"The MCP endpoint is off. Set {ListenerPortEnvVar} in rhino.compute's environment to serve it."
            });
        }

        static async Task WriteRpcErrorAsync(HttpResponse res, int status, JsonNode id, int code, string message)
        {
            res.StatusCode = status;
            res.ContentType = "application/json";
            var body = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id?.DeepClone(),
                ["error"] = new JsonObject { ["code"] = code, ["message"] = message },
            };
            await res.WriteAsync(body.ToJsonString());
        }
    }
}
