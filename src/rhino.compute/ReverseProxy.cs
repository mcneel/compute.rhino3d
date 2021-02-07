using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace rhino.compute
{
    public class ReverseProxyModule : Carter.CarterModule
    {
        HttpClient _client;

        public ReverseProxyModule()
        {
            _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");


            Get("/healthcheck", async (req, res) => await res.WriteAsync("healthy"));
            Get("/robots.txt", async (req, res) => await res.WriteAsync("User-agent: *\nDisallow: / "));
            Get("/", async (req, res) => await res.WriteAsync("compute.rhino3d"));

            // I would like to catch all other GET endpoints and just pass them on. Currently,
            // to lazy to figure out the nice way to do this
            Get("/{a}", ReverseProxyGet);
            Get("/{a}/{b}", ReverseProxyGet);
            Get("/{a}/{b}/{c}", ReverseProxyGet);
            Get("/{a}/{b}/{c}/{d}", ReverseProxyGet);
            Get("/{a}/{b}/{c}/{d}/{e}", ReverseProxyGet);

            Post("/grasshopper", ReverseProxyGrasshopper);
            Post("/{uri}", ReverseProxyPost);
        }

        async Task<HttpResponseMessage> SendProxyRequest(HttpRequest initialRequest, HttpMethod method)
        {
            string baseurl = ComputeChildren.GetComputeServerBaseUrl();
            string proxyUrl = $"{baseurl}{initialRequest.Path}{initialRequest.QueryString}";

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
            var stringResponse = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(stringResponse);
        }

        private async Task ReverseProxyPost(HttpRequest req, HttpResponse res)
        {
            var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
            var s = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(s);
        }

        private async Task ReverseProxyGrasshopper(HttpRequest req, HttpResponse res)
        {
            var proxyResponse = await SendProxyRequest(req, HttpMethod.Post);
            var s = await proxyResponse.Content.ReadAsStringAsync();
            await res.WriteAsync(s);
        }
    }
}
