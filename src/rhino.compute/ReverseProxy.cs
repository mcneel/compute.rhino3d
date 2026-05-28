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
            app.MapGet("/{*uri}", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Get));
            app.MapPost("/grasshopper", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Post));
            app.MapPost("/{*uri}", (HttpRequest req, HttpResponse res) => ProxyRequest(req, res, HttpMethod.Post));
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
                for (int i = 0; i < children; i++)
                {
                    ComputeChildren.LaunchCompute(false);
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
