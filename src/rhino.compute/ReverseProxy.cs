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
    public class ReverseProxyModule : Carter.ICarterModule
    {
        static int _initCalled = 0;
        static Task _initTask;
        static HttpClient _client;
        private const string _apiKeyHeader = "RhinoComputeKey";
        static void Initialize()
        {
            // Use an atomic compare-and-swap so that concurrent first requests cannot
            // both pass this guard and double-initialize the HttpClient or child processes.
            if (System.Threading.Interlocked.CompareExchange(ref _initCalled, 1, 0) != 0)
                return;

            Log.Debug($"Initializing reverse proxy at {DateTime.Now.ToLocalTime()}");

            // SocketsHttpHandler gives direct control over connection pool lifetime.
            // PooledConnectionIdleTimeout ensures we close idle connections to compute.geometry
            // before it closes them on its end, avoiding SocketExceptions in the pool scavenger.
            _client = new HttpClient(new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(60)
            });
            _client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");
            _client.Timeout = TimeSpan.FromSeconds(Config.ReverseProxyRequestTimeout);

            // Launch child processes on start. Getting the base url is enough to get things rolling
            if (ComputeChildren.SpawnOnStartup)
            {
                InitializeChildren();
            }
        }

        static void InitializeChildren()
        {
            ComputeChildren.UpdateLastCall();
            _initTask = Task.Run(() =>
            {
                var (url, port) = ComputeChildren.GetComputeServerBaseUrl();
                ComputeChildren.MoveToFrontOfQueue(port);
            });
        }

        static System.Timers.Timer _concurrentRequestLogger;
        static int _activeConcurrentRequests;
        static int _maxConcurrentRequests;
        class ConcurrentRequestTracker : System.IDisposable
        {
            public ConcurrentRequestTracker()
            {
                _activeConcurrentRequests++;
                if (_activeConcurrentRequests > _maxConcurrentRequests)
                    _maxConcurrentRequests = _activeConcurrentRequests;
            }

            public void Dispose()
            {
                _activeConcurrentRequests--;
            }
        }
        public static void InitializeConcurrentRequestLogging(Microsoft.Extensions.Logging.ILogger logger)
        {
            // log once per minute
            var span = new System.TimeSpan(0, 1, 0);
            _concurrentRequestLogger = new System.Timers.Timer(span.TotalMilliseconds);
            _concurrentRequestLogger.Elapsed += (s, e) =>
            {
                logger.LogInformation($"Max concurrent requests = {_maxConcurrentRequests}");
                _maxConcurrentRequests = _activeConcurrentRequests;
            };
            _concurrentRequestLogger.AutoReset = true;
            _concurrentRequestLogger.Start();
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/robots.txt", async (context) => await context.Response.WriteAsync("User-agent: *\nDisallow: / "));
            app.MapGet("/idlespan", async (context) => { Serilog.Log.Debug($"Request received to /idlespan endpoint"); await context.Response.WriteAsync($"{ComputeChildren.IdleSpan()}"); });
            app.MapGet("/", async (context) => { InitializeChildren(); await context.Response.WriteAsync("compute.rhino3d"); });
            app.MapGet("/activechildren", async (context) => { InitializeChildren(); await context.Response.WriteAsync($"{ComputeChildren.ActiveComputeCount}"); });
            app.MapGet("/launch", LaunchChildren);
            app.MapGet("/favicon.ico", async (context) => await context.Response.WriteAsync("Handled"));

            // routes that are proxied to compute.geometry
            app.MapGet("/{*uri}", ReverseProxyGet);
            app.MapPost("/grasshopper", ReverseProxyPost);
            app.MapPost("/{*uri}", ReverseProxyPost);
        }

        public ReverseProxyModule()
        {
            Initialize();
        }

        Task LaunchChildren(HttpRequest request, HttpResponse response)
        {
            int children = System.Convert.ToInt32(request.Query["children"]);
            int parentProcessId = System.Convert.ToInt32(request.Query["parent"]);
            if (Program.IsParentRhinoProcess(parentProcessId))
            {
                for (int i=0; i<children; i++)
                {
                    System.Threading.Tasks.Task.Run(() => ComputeChildren.LaunchCompute());
                }
            }
            return Task.CompletedTask;
        }

        async Task AwaitInitTask()
        {
            var task = _initTask;
            if (task != null)
            {
                await task;
                _initTask = null;
            }
        }

        async Task<HttpResponseMessage> SendProxyRequest(HttpRequest initialRequest, HttpMethod method, string baseurl)
        {
            string proxyUrl = $"{baseurl}{initialRequest.Path}{initialRequest.QueryString}";

            // mark the current time as a call to a compute child process
            ComputeChildren.UpdateLastCall();

            if (method == HttpMethod.Post)
            {
                // include RhinoComputeKey header in request to compute child process
                using var req = new HttpRequestMessage(HttpMethod.Post, proxyUrl);
                if (initialRequest.Headers.TryGetValue(_apiKeyHeader, out var keyHeader))
                    req.Headers.Add(_apiKeyHeader, keyHeader.ToString());

                // Stream the request body directly to the child process rather than
                // buffering it as a string, avoiding a full in-memory copy of the payload.
                var streamContent = new StreamContent(initialRequest.BodyReader.AsStream(leaveOpen: false));
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                req.Content = streamContent;
                // SendAsync fully consumes the request body before returning, so disposing
                // req (and its owned StreamContent) here is safe.
                return await _client.SendAsync(req);
            }

            if (method == HttpMethod.Get)
            {
                return await _client.GetAsync(proxyUrl);
            }

            throw new System.NotSupportedException("Only GET and POST are currently supported for reverse proxy");
        }

        // GET and POST routes both delegate to the shared handler; the /grasshopper route
        // is intentionally mapped to ReverseProxyPost as it requires no special handling.
        private async Task ReverseProxyGet(HttpRequest req, HttpResponse res)
            => await ReverseProxyHandler(req, res, HttpMethod.Get);

        private async Task ReverseProxyPost(HttpRequest req, HttpResponse res)
            => await ReverseProxyHandler(req, res, HttpMethod.Post);

        // Shared proxy handler: forwards the request to a compute.geometry child, propagates
        // the response status code, and promotes the responding child to the front of the queue
        // on success so it is preferred for the next round-robin selection.
        private async Task ReverseProxyHandler(HttpRequest req, HttpResponse res, HttpMethod method)
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
