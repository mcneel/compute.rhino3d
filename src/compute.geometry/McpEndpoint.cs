using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace compute.geometry
{
    // POST /mcp, mapped while the usage log is on: an MCP call rhino.compute passes on to the RhinoAI plug-in's listener
    // in this process (see McpForwarding), measured and recorded like any other billable request, with what the call
    // was in its record's mcp field. Opening and closing a session's document happen here too, for the client asking.
    static class McpEndpoint
    {
        static readonly HttpClient client = new HttpClient(new SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false })
        {
            // rhino.compute's timeout applies; when it gives up, this request is aborted and the call with it.
            Timeout = Timeout.InfiniteTimeSpan,
        };
        // The plug-in's listener table isn't safe for two control calls at once.
        static readonly SemaphoreSlim controlLock = new SemaphoreSlim(1, 1);
        static readonly TimeSpan CONTROL_TIMEOUT = TimeSpan.FromSeconds(30);

        public static void MapEndpoints(IEndpointRouteBuilder app) => app.MapPost(McpForwarding.PATH, Forward).Billable();

        // The startup document's listener, on the port rhino.compute gave this process.
        static int BasePort => int.TryParse(Environment.GetEnvironmentVariable("RHINO_MCP_AUTOSTART_PORT"), out int port) && port > 0 ? port : 0;

        static async Task Forward(HttpContext context)
        {
            var request = context.Request;
            var ct = context.RequestAborted;
            int basePort = BasePort;
            // Only a child of rhino.compute has sessions; rhino.compute keeps them.
            if (basePort == 0 || !(Shutdown.ParentProcesses?.Count > 0))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            string session = request.Headers[McpForwarding.SESSION_HEADER].ToString();
            if (session.Length == 0)
                session = null;

            if (int.TryParse(request.Headers[McpForwarding.CLOSE_HEADER], NumberStyles.None, CultureInfo.InvariantCulture, out int closing))
            {
                string failure = null;
                try
                {
                    await ControlAsync(basePort, "_router_close_listener", new JsonObject { ["port"] = closing }, ct);
                }
                catch (Exception ex) when (IsCallFailure(ex, ct))
                {
                    failure = ex.Message;
                }
                RequestMcp.Set(context, session, "session/close", null, failure);
                context.Response.StatusCode = failure == null ? StatusCodes.Status200OK : StatusCodes.Status502BadGateway;
                await context.Response.WriteAsJsonAsync(failure == null ? new { closed = true } : (object)new { error = failure }, ct);
                return;
            }

            byte[] body;
            using (var buffer = new MemoryStream())
            {
                await request.Body.CopyToAsync(buffer, ct);
                body = buffer.ToArray();
            }
            var (method, tool, id) = Describe(body);

            int port;
            if (request.Headers.ContainsKey(McpForwarding.OPEN_HEADER))
            {
                try
                {
                    port = await SpawnAsync(basePort, ct);
                }
                catch (Exception ex) when (IsCallFailure(ex, ct))
                {
                    string message = $"Could not open a document for the session: {ex.Message}";
                    RequestMcp.Set(context, session, method, tool, message);
                    await WriteRpcErrorAsync(context, StatusCodes.Status503ServiceUnavailable, id, message);
                    return;
                }
                context.Response.Headers[McpForwarding.LISTENER_HEADER] = port.ToString(CultureInfo.InvariantCulture);
            }
            else if (!int.TryParse(request.Headers[McpForwarding.LISTENER_HEADER], NumberStyles.None, CultureInfo.InvariantCulture, out port)
                || port < basePort || port >= basePort + McpForwarding.PORTS_PER_CHILD)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var upstream = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/") { Content = new ByteArrayContent(body) };
            upstream.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            if (request.Headers.TryGetValue("Accept", out var accept))
                upstream.Headers.TryAddWithoutValidation("Accept", accept.ToArray());
            HttpResponseMessage answer;
            try
            {
                answer = await client.SendAsync(upstream, ct);
            }
            catch (HttpRequestException ex)
            {
                string message = $"The RhinoAI listener on port {port} didn't answer: {ex.Message}";
                RequestMcp.Set(context, session, method, tool, message);
                await WriteRpcErrorAsync(context, StatusCodes.Status502BadGateway, id, message);
                return;
            }
            using (answer)
            {
                byte[] payload = await answer.Content.ReadAsByteArrayAsync(ct);
                if (method == "tools/list" && answer.IsSuccessStatusCode)
                    payload = StripRouterTools(payload);
                RequestMcp.Set(context, session, method, tool, Failure(payload));
                context.Response.StatusCode = (int)answer.StatusCode;
                if (answer.Content.Headers.ContentType != null)
                    context.Response.ContentType = answer.Content.Headers.ContentType.ToString();
                if (payload.Length > 0)
                    await context.Response.Body.WriteAsync(payload, ct);
            }
        }

        static bool IsCallFailure(Exception ex, CancellationToken ct) =>
            ex is HttpRequestException || ex is InvalidOperationException || ex is JsonException || (ex is OperationCanceledException && !ct.IsCancellationRequested);

        // The JSON-RPC method, the tool a tools/call names, and the id to answer an error with.
        static (string Method, string Tool, JsonNode Id) Describe(byte[] body)
        {
            try
            {
                using var json = JsonDocument.Parse(body);
                var root = json.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                    return ("batch", null, null);
                if (root.ValueKind != JsonValueKind.Object)
                    return (null, null, null);
                string method = root.TryGetProperty("method", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null;
                string tool = method == "tools/call" && root.TryGetProperty("params", out var p) && p.ValueKind == JsonValueKind.Object
                    && p.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null;
                JsonNode id = root.TryGetProperty("id", out var i) ? JsonNode.Parse(i.GetRawText()) : null;
                return (method, tool, id);
            }
            catch (JsonException)
            {
                return (null, null, null);
            }
        }

        // Why the call failed, when its answer is a JSON-RPC error or a tool's error.
        static string Failure(byte[] payload)
        {
            try
            {
                using var json = JsonDocument.Parse(payload);
                var root = json.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;
                if (root.TryGetProperty("error", out var error))
                    return error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String
                        ? message.GetString() : error.GetRawText();
                if (root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object
                    && result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
                    return FirstText(result) ?? "The tool failed.";
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        static async Task<int> SpawnAsync(int basePort, CancellationToken ct)
        {
            string text = await ControlAsync(basePort, "_router_spawn_listener", new JsonObject(), ct);
            using var json = JsonDocument.Parse(text);
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("port", out var p) && p.TryGetInt32(out int port) && port > 0)
                return port;
            throw new InvalidOperationException($"_router_spawn_listener returned no port: {text}");
        }

        // A tools/call of one of the plug-in's control tools on the startup document's listener; the text of the first
        // content block, which those tools fill with a JSON object.
        static async Task<string> ControlAsync(int basePort, string tool, JsonObject arguments, CancellationToken ct)
        {
            var call = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = Guid.NewGuid().ToString("N"),
                ["method"] = "tools/call",
                ["params"] = new JsonObject { ["name"] = tool, ["arguments"] = arguments },
            };
            await controlLock.WaitAsync(ct);
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(CONTROL_TIMEOUT);
                using var message = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{basePort}/")
                {
                    Content = new StringContent(call.ToJsonString(), Encoding.UTF8, "application/json"),
                };
                using var response = await client.SendAsync(message, timeout.Token);
                string body = await response.Content.ReadAsStringAsync(timeout.Token);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"{tool} returned HTTP {(int)response.StatusCode}: {body}");
                using var json = JsonDocument.Parse(body);
                var root = json.RootElement;
                if (root.TryGetProperty("error", out var error))
                    throw new InvalidOperationException($"{tool} failed: {error.GetRawText()}");
                if (!root.TryGetProperty("result", out var result))
                    throw new InvalidOperationException($"{tool} returned no result: {body}");
                if (result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
                    throw new InvalidOperationException($"{tool} failed: {FirstText(result)}");
                return FirstText(result) ?? throw new InvalidOperationException($"{tool} returned no text content: {body}");
            }
            finally
            {
                controlLock.Release();
            }
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

        // Agents don't get the plug-in's control tools.
        static byte[] StripRouterTools(byte[] payload)
        {
            try
            {
                var node = JsonNode.Parse(payload);
                if (node?["result"]?["tools"] is not JsonArray tools)
                    return payload;
                var hidden = tools.Where(t => t?["name"]?.GetValue<string>() is string name && name.StartsWith(McpForwarding.ROUTER_TOOL_PREFIX, StringComparison.Ordinal)).ToArray();
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

        static async Task WriteRpcErrorAsync(HttpContext context, int status, JsonNode id, string message)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            var body = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id?.DeepClone(),
                ["error"] = new JsonObject { ["code"] = -32000, ["message"] = message },
            };
            await context.Response.WriteAsync(body.ToJsonString());
        }
    }
}
