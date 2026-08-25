using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Serilog;

namespace rhino.compute
{
    public class ReverseProxyModule
    {
        static bool initCalled = false;
        static Task initTask;
        static HttpClient client;
        private const string API_KEY_HEADER = "RhinoComputeKey";

        static void Initialize()
        {
            if (initCalled)
                return;
            initCalled = true;

            Log.Debug($"Initiliazing reverse proxy at {DateTime.Now.ToLocalTime()}");

            client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");
            client.Timeout = TimeSpan.FromSeconds(Config.ReverseProxyRequestTimeout);

            // Launch child processes on start. Getting the base url is enough to get things rolling
            if (ComputeChildren.SpawnOnStartup)
            {
                InitializeChildren();
            }
        }

        static void InitializeChildren()
        {
            ComputeChildren.UpdateLastCall();
            initTask = Task.Run(() =>
            {
                var (url, port) = ComputeChildren.GetComputeServerBaseUrl();
                ComputeChildren.MoveToFrontOfQueue(port);
            });
        }

        static System.Timers.Timer concurrentRequestLogger;
        static int activeConcurrentRequests;
        static int maxConcurrentRequests;
        class ConcurrentRequestTracker : System.IDisposable
        {
            public ConcurrentRequestTracker()
            {
                activeConcurrentRequests++;
                if (activeConcurrentRequests > maxConcurrentRequests)
                    maxConcurrentRequests = activeConcurrentRequests;
            }

            public void Dispose()
            {
                activeConcurrentRequests--;
            }
        }
        public static void InitializeConcurrentRequestLogging(Microsoft.Extensions.Logging.ILogger logger)
        {
            // log once per minute
            var span = new System.TimeSpan(0, 1, 0);
            concurrentRequestLogger = new System.Timers.Timer(span.TotalMilliseconds);
            concurrentRequestLogger.Elapsed += (s, e) =>
            {
                logger.LogInformation($"Max concurrent requests = {maxConcurrentRequests}");
                maxConcurrentRequests = activeConcurrentRequests;
            };
            concurrentRequestLogger.AutoReset = true;
            concurrentRequestLogger.Start();
        }

        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            Initialize();

            app.MapGet("/robots.txt", async (context) => await context.Response.WriteAsync("User-agent: *\nDisallow: / "));
            app.MapGet("/idlespan", async (context) => { Serilog.Log.Debug($"Request received to /idlespan endpoint"); await context.Response.WriteAsync($"{ComputeChildren.IdleSpan()}"); });
            app.MapGet("/", (context) =>
            {
                InitializeChildren();
                context.Response.Redirect("https://www.rhino3d.com/compute");
                return Task.CompletedTask;
            });
            // /activechildren is the long-standing slug-style endpoint name. /active-children is a
            // kebab-case alias matching the style of the newer /shutdown-children, /recycle-children
            // etc. Both routes share the same handler. The original slug stays for backward compat.
            app.MapGet("/activechildren", ActiveChildrenEndpoint);
            app.MapGet("/active-children", ActiveChildrenEndpoint);
            app.MapGet("/launch", LaunchChildren);
            app.MapGet("/favicon.ico", async (context) => await context.Response.WriteAsync("Handled"));

            // Child-lifecycle management endpoints. All POST so they're auth-gated by the
            // existing ApiKeyMiddleware when RHINO_COMPUTE_KEY is configured. Registered before
            // the catch-all proxy route below so they're not forwarded to a compute.geometry child.
            app.MapPost("/shutdown-children", ShutdownChildrenEndpoint);
            app.MapPost("/recycle-children",  RecycleChildrenEndpoint);
            app.MapPost("/launch-children",   LaunchChildrenEndpoint);
            app.MapPost("/launch-child",      LaunchChildEndpoint);

            // Guard against accidental hits on the internal /shutdown endpoint. Without this,
            // the catch-all proxy below would forward POST /shutdown to one round-robin-selected
            // compute.geometry child, silently killing it (it would respawn on the next
            // /grasshopper request, but the side-effect is surprising). The 400 with a hint
            // points users at the right endpoint instead.
            app.MapPost("/shutdown", async (HttpResponse res) =>
            {
                res.StatusCode = 400;
                await res.WriteAsJsonAsync(new
                {
                    error = "POST /shutdown is internal-only (rhino.compute → compute.geometry). " +
                            "Use POST /shutdown-children to shut down children from outside the cluster."
                });
            });

            // routes that are proxied to compute.geometry
            app.MapGet("/{*uri}", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Get));
            app.MapPost("/grasshopper", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Post));
            app.MapPost("/{*uri}", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Post));
        }

        // GET /activechildren and GET /active-children — report the number of compute.geometry
        // children READY TO SERVE (port open, in the pool) as a plain integer body. Always 200:
        // this is a query that always succeeds and "0" is a valid answer (no children ready yet),
        // so a 503 would wrongly imply the endpoint itself failed. Callers treat > 0 as ready.
        // Pure report, NO side effects: it never spawns a child, so polling it can never start
        // the metered software charge. To launch children use POST /launch-children (fill to
        // SpawnCount) or POST /launch-child (add one).
        //
        // NOTE: the count is the READY pool (ComputeChildren.CurrentChildCount), not the raw OS
        // process count — a child still loading Rhino is not counted until its port is open.
        static async Task ActiveChildrenEndpoint(HttpContext context)
        {
            await context.Response.WriteAsync($"{ComputeChildren.CurrentChildCount}");
        }

        // POST /shutdown-children — gracefully shut down children. No params = all; ?port=N
        // = just that one. Does not respawn. After shutdown-all, the next /grasshopper request
        // triggers an auto-spawn back to SpawnCount; to confirm the ready count, poll
        // /activechildren (it never spawns).
        static async Task ShutdownChildrenEndpoint(HttpRequest req, HttpResponse res)
        {
            if (!TryParsePortFilter(req, out int? portFilter, out string parseError))
            {
                res.StatusCode = 400;
                await res.WriteAsJsonAsync(new { error = parseError });
                return;
            }
            var (shutdown, _) = ComputeChildren.ShutdownChildren(portFilter, respawn: false);
            await res.WriteAsJsonAsync(new
            {
                shutdown,
                active = ComputeChildren.CurrentChildCount,
            });
        }

        // POST /recycle-children — shut down + respawn. No params = all; ?port=N = just that one.
        // Sequential (waits for each replacement to be serving before moving on) so the queue
        // never drops to zero mid-recycle when other children are handling traffic.
        static async Task RecycleChildrenEndpoint(HttpRequest req, HttpResponse res)
        {
            if (!TryParsePortFilter(req, out int? portFilter, out string parseError))
            {
                res.StatusCode = 400;
                await res.WriteAsJsonAsync(new { error = parseError });
                return;
            }
            var (shutdown, spawned) = ComputeChildren.ShutdownChildren(portFilter, respawn: true);
            await res.WriteAsJsonAsync(new
            {
                shutdown,
                spawned,
                active = ComputeChildren.CurrentChildCount,
            });
        }

        // POST /launch-children — fill the pool up to SpawnCount. No-op when already at or above.
        // To raise capacity permanently, restart rhino.compute with a higher --childcount;
        // /launch-children only fills to the configured baseline.
        static async Task LaunchChildrenEndpoint(HttpRequest req, HttpResponse res)
        {
            var spawned = ComputeChildren.LaunchChildren();
            await res.WriteAsJsonAsync(new
            {
                spawned,
                active = ComputeChildren.CurrentChildCount,
            });
        }

        // POST /launch-child — add one child to the pool. Can push above SpawnCount up to the
        // MaxChildren cap. ?port=N requests a specific port; otherwise picks the next free one.
        // 400 = malformed/out-of-range port. 409 = port in use. 503 = at MaxChildren.
        static async Task LaunchChildEndpoint(HttpRequest req, HttpResponse res)
        {
            int? requestedPort = null;
            if (req.Query.TryGetValue("port", out var portValue) && !string.IsNullOrEmpty(portValue))
            {
                if (!int.TryParse(portValue, out int parsed))
                {
                    res.StatusCode = 400;
                    await res.WriteAsJsonAsync(new { error = "Port must be an integer." });
                    return;
                }
                requestedPort = parsed;
            }
            try
            {
                int port = ComputeChildren.LaunchChild(requestedPort);
                await res.WriteAsJsonAsync(new { spawned = new[] { port } });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                res.StatusCode = 400;
                await res.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Distinguish "max children reached" (503) from "port in use" or "no port available" (409).
                res.StatusCode = ex.Message.StartsWith("Maximum child count reached") ? 503 : 409;
                await res.WriteAsJsonAsync(new { error = ex.Message });
            }
        }

        // Parses optional ?port=N from the query. Returns false with errorMessage when the
        // value is present but unparseable or out of range. Returns true with portFilter=null
        // when no port parameter was supplied (caller should operate on all children).
        static bool TryParsePortFilter(HttpRequest req, out int? portFilter, out string errorMessage)
        {
            portFilter = null;
            errorMessage = null;
            if (!req.Query.TryGetValue("port", out var portValue) || string.IsNullOrEmpty(portValue))
                return true;
            if (!int.TryParse(portValue, out int parsed))
            {
                errorMessage = "Port must be an integer.";
                return false;
            }
            if (parsed < 6001 || parsed > 65535)
            {
                errorMessage = "Port must be in range 6001-65535.";
                return false;
            }
            portFilter = parsed;
            return true;
        }

        static async Task LaunchChildren(HttpRequest request, HttpResponse response)
        {
            // Reject malformed input cleanly instead of letting Convert.ToInt32 throw a
            // FormatException that would bubble up to the global exception handler.
            if (!int.TryParse(request.Query["children"], out int children) || children <= 0)
            {
                response.StatusCode = 400;
                await response.WriteAsync("children query parameter must be a positive integer");
                return;
            }
            if (!int.TryParse(request.Query["parent"], out int parentProcessId))
            {
                response.StatusCode = 400;
                await response.WriteAsync("parent query parameter must be an integer");
                return;
            }
            if (children > ComputeChildren.MaxChildren)
            {
                Log.Warning("/launch capped from {Requested} to {Cap} children", children, ComputeChildren.MaxChildren);
                children = ComputeChildren.MaxChildren;
            }
            if (Program.IsParentRhinoProcess(parentProcessId))
            {
                // LaunchCompute() blocks until the spawned child is ready. The legacy semantics
                // here are fire-and-forget — Rhino startup doesn't want to wait — so dispatch
                // each launch to a background task.
                for (int i = 0; i < children; i++)
                {
                    _ = System.Threading.Tasks.Task.Run(() => ComputeChildren.LaunchCompute());
                }
            }
        }

        static async Task AwaitInitTask()
        {
            var task = initTask;
            if (task != null)
            {
                await task;
                initTask = null;
            }
        }

        static async Task<HttpResponseMessage> SendProxyRequest(HttpRequest initialRequest, HttpMethod method, string baseurl)
        {
            string proxyUrl = $"{baseurl}{initialRequest.Path}{initialRequest.QueryString}";

            // mark the current time as a call to a compute child process
            ComputeChildren.UpdateLastCall();

            if (method == HttpMethod.Post)
            {
                // include RhinoComputeKey header in request to compute child process
                var req = new HttpRequestMessage(HttpMethod.Post, proxyUrl);
                if (initialRequest.Headers.TryGetValue(API_KEY_HEADER, out var keyHeader))
                    req.Headers.Add(API_KEY_HEADER, keyHeader.ToString());

                using (var sw = new System.IO.StreamReader(initialRequest.BodyReader.AsStream()))
                {
                    string body = sw.ReadToEnd();
                    using (var stringContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json"))
                    {
                        req.Content = stringContent;
                        return await client.SendAsync(req);
                    }
                }
            }

            if (method == HttpMethod.Get)
            {
                return await client.GetAsync(proxyUrl);
            }

            throw new System.NotSupportedException("Only GET and POST are currently supported for reverse proxy");
        }

        static async Task ProxyRequest(HttpRequest req, HttpResponse res, HttpMethod method)
        {
            await AwaitInitTask();
            using (var tracker = new ConcurrentRequestTracker())
            {
                var (baseurl, port) = ComputeChildren.GetComputeServerBaseUrl();
                using var proxyResponse = await SendProxyRequest(req, method, baseurl);
                ComputeChildren.UpdateLastCall();
                if (proxyResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    ComputeChildren.MoveToFrontOfQueue(port);

                res.StatusCode = (int)proxyResponse.StatusCode;
                // Forward the upstream Content-Type so JSON responses arrive at the caller as
                // application/json rather than the ASP.NET Core default text/plain.
                if (proxyResponse.Content.Headers.ContentType != null)
                    res.ContentType = proxyResponse.Content.Headers.ContentType.ToString();
                var responseString = await proxyResponse.Content.ReadAsStringAsync();
                await res.WriteAsync(responseString);
            }
        }
    }
}
