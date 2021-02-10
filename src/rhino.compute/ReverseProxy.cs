using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rhino.compute
{
    public class ReverseProxyModule : Carter.CarterModule
    {
        static bool _initCalled = false;
        static HttpClient _client;
        static void Initialize()
        {
            if (_initCalled)
                return;
            _initCalled = true;

            // Port that rhino.compute is running on
            // todo: figure out how to programatically determine this port
            ComputeChildren.ParentPort = 5000;
            // Set idle time child processes live. If rhino.compute is not called
            // for this period of time to proxy requests, the child processes will
            // shut down. The processes will be restarted on a later request
            ComputeChildren.ChildIdleSpan = new System.TimeSpan(1, 0, 0);
            // Number of child compute.geometry processes to start for processing.
            // Default to 4 instances, but we might want to set this based on the
            // number of cores available on the computer.
            ComputeChildren.SpawnCount = 4;

            _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");

            // Launch child processes on start. Getting the base url is enough to get things rolling
            ComputeChildren.UpdateLastCall();
            Task.Run(() => ComputeChildren.GetComputeServerBaseUrl());
        }

        public ReverseProxyModule()
        {
            Get("/healthcheck", async (req, res) => await res.WriteAsync("healthy"));
            Get("/robots.txt", async (req, res) => await res.WriteAsync("User-agent: *\nDisallow: / "));
            Get("/idlespan", async (req, res) => await res.WriteAsync($"{ComputeChildren.IdleSpan()}"));
            Get("/", async (req, res) => await res.WriteAsync("compute.rhino3d"));

            // routes that are proxied to compute.geometry
            Get("/{*uri}", ReverseProxyGet);
            Post("/grasshopper", ReverseProxyGrasshopper);
            Post("/{*uri}", ReverseProxyPost);

            Initialize();
        }

        async Task<HttpResponseMessage> SendProxyRequest(HttpRequest initialRequest, HttpMethod method)
        {
            string baseurl = ComputeChildren.GetComputeServerBaseUrl();
            string proxyUrl = $"{baseurl}{initialRequest.Path}{initialRequest.QueryString}";

            // mark the current time as a call to a compute child process
            ComputeChildren.UpdateLastCall();

            if (method == HttpMethod.Post)
            {
                using (var stream = initialRequest.BodyReader.AsStream(false))
                {
                    using (var sw = new System.IO.StreamReader(initialRequest.BodyReader.AsStream()))
                    {
                        string body = sw.ReadToEnd();
                        using (var stringContent = new StringContent(body, System.Text.Encoding.UTF8, "applicaton/json"))
                        {
                            return await _client.PostAsync(proxyUrl, stringContent);
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
            var proxyResponse = await SendProxyRequest(req, HttpMethod.Get);
            ComputeChildren.UpdateLastCall();
            var stringResponse = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(stringResponse);
        }

        private async Task ReverseProxyPost(HttpRequest req, HttpResponse res)
        {
            var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
            ComputeChildren.UpdateLastCall();
            var s = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(s);
        }

        private async Task ReverseProxyGrasshopper(HttpRequest req, HttpResponse res)
        {
            var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
            ComputeChildren.UpdateLastCall();
            var s = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(s);
        }
    }
}
