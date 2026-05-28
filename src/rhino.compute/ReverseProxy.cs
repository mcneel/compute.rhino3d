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
        static int initCalled = 0;
        static Task initTask;
        static HttpClient client;
        private const string API_KEY_HEADER = "RhinoComputeKey";
        static void Initialize()
        {
            // Use an atomic compare-and-swap so that concurrent first requests cannot
            // both pass this guard and double-initialize the HttpClient or child processes.
            if (System.Threading.Interlocked.CompareExchange(ref initCalled, 1, 0) != 0)
                return;

            Log.Debug($"Initializing reverse proxy at {DateTime.Now.ToLocalTime()}");

            // SocketsHttpHandler gives direct control over connection pool lifetime.
            // PooledConnectionIdleTimeout ensures we close idle connections to compute.geometry
            // before it closes them on its end, avoiding SocketExceptions in the pool scavenger.
            client = new HttpClient(new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(60)
            });
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
            app.MapGet("/activechildren", async (context) =>
            {
                bool initialize = true;
                if (context.Request.Query.TryGetValue("initialize", out var initValue)
                    && bool.TryParse(initValue, out var parsed))
                {
                    initialize = parsed;
                }
                if (initialize)
                    InitializeChildren();
                await context.Response.WriteAsync($"{ComputeChildren.ActiveComputeCount}");
            });
            app.MapGet("/launch", LaunchChildren);
            app.MapGet("/favicon.ico", async (context) => await context.Response.WriteAsync("Handled"));

            // routes that are proxied to compute.geometry
            app.MapGet("/{*uri}", ReverseProxyGet);
            app.MapPost("/grasshopper", ReverseProxyPost);
            app.MapPost("/{*uri}", ReverseProxyPost);
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
                for (int i=0; i<children; i++)
                {
                    // Fire-and-forget: spawn children in the background and return immediately,
                    // matching the original non-async behavior. Discard documents the intent
                    // and silences CS4014 now that the enclosing method is async.
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
                using var req = new HttpRequestMessage(HttpMethod.Post, proxyUrl);
                if (initialRequest.Headers.TryGetValue(API_KEY_HEADER, out var keyHeader))
                    req.Headers.Add(API_KEY_HEADER, keyHeader.ToString());

                // Stream the request body directly to the child process rather than
                // buffering it as a string, avoiding a full in-memory copy of the payload.
                var streamContent = new StreamContent(initialRequest.BodyReader.AsStream(leaveOpen: false));
                if (!string.IsNullOrWhiteSpace(initialRequest.ContentType) &&
                    System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(initialRequest.ContentType, out var parsedContentType))
                    streamContent.Headers.ContentType = parsedContentType;
                else
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                req.Content = streamContent;
                // SendAsync fully consumes the request body before returning, so disposing
                // req (and its owned StreamContent) here is safe.
                return await client.SendAsync(req);
            }

            if (method == HttpMethod.Get)
            {
                return await client.GetAsync(proxyUrl);
            }

            throw new System.NotSupportedException("Only GET and POST are currently supported for reverse proxy");
        }

        // GET and POST routes both delegate to the shared handler; the /grasshopper route
        // is intentionally mapped to ReverseProxyPost as it requires no special handling.
        private static async Task ReverseProxyGet(HttpRequest req, HttpResponse res)
            => await ReverseProxyHandler(req, res, HttpMethod.Get);

        private static async Task ReverseProxyPost(HttpRequest req, HttpResponse res)
            => await ReverseProxyHandler(req, res, HttpMethod.Post);

        // Shared proxy handler: forwards the request to a compute.geometry child, propagates
        // the response status code, and promotes the responding child to the front of the queue
        // on success so it is preferred for the next round-robin selection.
        private static async Task ReverseProxyHandler(HttpRequest req, HttpResponse res, HttpMethod method)
        {
            await AwaitInitTask();
            string responseString;
            try
            {
                using (new ConcurrentRequestTracker())
                {
                    var (baseurl, port) = ComputeChildren.GetComputeServerBaseUrl();
                    using (var proxyResponse = await SendProxyRequest(req, method, baseurl))
                    {
                        ComputeChildren.UpdateLastCall();
                        if (proxyResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            ComputeChildren.MoveToFrontOfQueue(port);

                        res.StatusCode = (int)proxyResponse.StatusCode;
                        responseString = await proxyResponse.Content.ReadAsStringAsync();
                    }
                }
                await res.WriteAsync(responseString);
            }
            catch (Exception ex) when (ex is Microsoft.AspNetCore.Connections.ConnectionResetException ||
                                       ex is OperationCanceledException)
            {
                if (ex is OperationCanceledException && !req.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    // HttpClient timeout (TaskCanceledException extends OperationCanceledException):
                    // the client did not cancel — return 504 so the caller knows the backend didn't respond in time.
                    Log.Warning("{Method} request to compute child timed out: {Path}", method, req.Path);
                    res.StatusCode = StatusCodes.Status504GatewayTimeout;
                    await res.WriteAsync("Gateway timeout: compute child did not respond in time.");
                }
                else
                {
                    Log.Debug("{Method} request cancelled or connection reset by client: {Path}", method, req.Path);
                }
            }
        }
    }
}
