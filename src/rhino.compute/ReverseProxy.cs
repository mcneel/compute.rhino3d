using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace rhino.compute
{
    public class ReverseProxyModule : Carter.CarterModule
    {
        static bool _initCalled = false;
        static Task<string> _initTask;
        static HttpClient _client;
        private const string _apiKeyHeader = "RhinoComputeKey";
        static void Initialize()
        {
            if (_initCalled)
                return;
            _initCalled = true;

            _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");

            // Launch child processes on start. Getting the base url is enough to get things rolling
            ComputeChildren.UpdateLastCall();
            _initTask = Task.Run(() => ComputeChildren.GetComputeServerBaseUrl());
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
        public static void InitializeConcurrentRequestLogging(ILogger logger)
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

        public ReverseProxyModule()
        {
            Get("/healthcheck", async (req, res) => await res.WriteAsync("healthy"));
            Get("/robots.txt", async (req, res) => await res.WriteAsync("User-agent: *\nDisallow: / "));
            Get("/idlespan", async (req, res) => await res.WriteAsync($"{ComputeChildren.IdleSpan()}"));
            Get("/", async (req, res) => await res.WriteAsync("compute.rhino3d"));
            Get("/launch", LaunchChildren);

            // routes that are proxied to compute.geometry
            Get("/{*uri}", ReverseProxyGet);
            Post("/grasshopper", ReverseProxyGrasshopper);
            Post("/{*uri}", ReverseProxyPost);

            Initialize();
        }

        Task LaunchChildren(HttpRequest request, HttpResponse response)
        {
            int children = System.Convert.ToInt32(request.Query["children"]);
            int parentProcessId = System.Convert.ToInt32(request.Query["parent"]);
            if (Program.IsParentRhinoProcess(parentProcessId))
            {
                for(int i=0; i<children; i++)
                {
                    ComputeChildren.LaunchCompute(false);
                }
            }
            return null;
        }

        async Task<HttpResponseMessage> SendProxyRequest(HttpRequest initialRequest, HttpMethod method)
        {
            var task = _initTask;
            if (task != null)
            {
                await task;
                _initTask = null;
            }

            string baseurl = ComputeChildren.GetComputeServerBaseUrl();
            string proxyUrl = $"{baseurl}{initialRequest.Path}{initialRequest.QueryString}";

            // mark the current time as a call to a compute child process
            ComputeChildren.UpdateLastCall();

            if (method == HttpMethod.Post)
            {
                // include RhinoComputeKey header in request to compute child process
                var req = new HttpRequestMessage(HttpMethod.Post, proxyUrl);
                if (initialRequest.Headers.TryGetValue(_apiKeyHeader, out var keyHeader))
                    req.Headers.Add(_apiKeyHeader, keyHeader.ToString());

                using (var stream = initialRequest.BodyReader.AsStream(false))
                {
                    using (var sw = new System.IO.StreamReader(initialRequest.BodyReader.AsStream()))
                    {
                        string body = sw.ReadToEnd();
                        using (var stringContent = new StringContent(body, System.Text.Encoding.UTF8, "applicaton/json"))
                        {
                            req.Content = stringContent;
                            return await _client.SendAsync(req);
                        }
                    }
                }
            }

            if (method == HttpMethod.Get)
            {
                return await _client.GetAsync(proxyUrl);
            }

            throw new System.NotSupportedException("Only GET and POST are currently supported for reverse proxy");
        }

        private async Task ReverseProxyGet(HttpRequest req, HttpResponse res)
        {
            string responseString;
            using (var tracker = new ConcurrentRequestTracker())
            {
                var proxyResponse = await SendProxyRequest(req, HttpMethod.Get);
                ComputeChildren.UpdateLastCall();
                responseString = await proxyResponse.Content.ReadAsStringAsync();
            }
            await res.WriteAsync(responseString);
        }

        private async Task ReverseProxyPost(HttpRequest req, HttpResponse res)
        {
            string responseString;
            using (var tracker = new ConcurrentRequestTracker())
            {
                var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
                ComputeChildren.UpdateLastCall();
                res.StatusCode = (int)proxyResponse.StatusCode;
                responseString = await proxyResponse.Content.ReadAsStringAsync();
            }
            await res.WriteAsync(responseString);
        }

        private async Task ReverseProxyGrasshopper(HttpRequest req, HttpResponse res)
        {
            string responseString;
            using (var tracker = new ConcurrentRequestTracker())
            {
                var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
                ComputeChildren.UpdateLastCall();
                res.StatusCode = (int)proxyResponse.StatusCode;
                responseString = await proxyResponse.Content.ReadAsStringAsync();
            }
            await res.WriteAsync(responseString);
        }
    }
}
