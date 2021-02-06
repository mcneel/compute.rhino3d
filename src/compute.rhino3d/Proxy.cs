using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace compute.rhino3d
{
    public class ProxyModule : Carter.CarterModule
    {
        HttpClient _client;

        public ProxyModule()
        {
            _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d-proxy/1.0.0");


            Get("/healthcheck", async (req, res) => await res.WriteAsync("healthy"));
            Get("/robots.txt", async (req, res) => await res.WriteAsync("User-agent: *\nDisallow: / "));
            Get("/", async (req, res) => await res.WriteAsync("compute.rhino3d"));
            Get("/{uri}", ProxyGetHandler);
            Post("/grasshopper", PostGrasshopperHandler);
            Post("/{uri}", PostHandler);
        }

        private async Task ProxyGetHandler(HttpRequest req, HttpResponse res)
        {
            //string url = ComputeChildren.GetComputeServerBaseUrl();
            //_client.SendAsync(req);

            await res.WriteAsync("GET " + req.Path);
        }

        private async Task PostHandler(HttpRequest req, HttpResponse res)
        {
            await res.WriteAsync("POST " + req.Path);
        }

        private async Task PostGrasshopperHandler(HttpRequest req, HttpResponse res)
        {
            await res.WriteAsync("POST grasshopper " + req.Path);
        }

        private readonly string[] _skipHeaders = new string[]
        {
            "Host",         // causes 400 invalid host
            "User-Agent",   // occasionally throws System.FormatException
            "Authorization" // frontend only
        };

        //private async Task<Response> DoRequest(NancyContext ctx, HttpRequestMessage req)
        //{
        //    // proxy headers
        //    foreach (var header in ctx.Request.Headers)
        //    {
        //        if (_skipHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase) ||
        //            header.Key.ToLower().StartsWith("content")) // content headers handled below
        //            continue;

        //        if (!req.Headers.TryAddWithoutValidation(header.Key, header.Value))
        //            Log.Warning($"Couldn't pass header '{header.Key}: {header.Value}' to backend");
        //    }

        //    if (req.Content != null)
        //    {
        //        if (MediaTypeHeaderValue.TryParse(ctx.Request.Headers.ContentType, out var contentType))
        //            req.Content.Headers.ContentType = contentType;
        //        else
        //        {
        //            Log.Warning($"Couldn't pass invalid Content-Type '{ctx.Request.Headers.ContentType}' to backend");
        //            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        }
        //        req.Content.Headers.ContentLength = ctx.Request.Headers.ContentLength;
        //    }

        //    // add compute-specific headers
        //    req.Headers.Add("X-Compute-Id", Context.Items["RequestId"] as string);
        //    req.Headers.Add("X-Compute-Host", Context.Items["Hostname"] as string);

        //    try
        //    {
        //        var backendResponse = await _client.SendAsync(req);
        //        return CreateProxyResponse(backendResponse, ctx);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "An exception occured while proxying request \"{RequestId}\" to the backend", Context.Items["RequestId"] as string);
        //        return new Nancy.Responses.TextResponse(Nancy.HttpStatusCode.InternalServerError, "{ \"message\": \"Backend not available\" }");
        //    }
        //}

        //Nancy.Response CreateProxyResponse(HttpResponseMessage backendResponse, Nancy.NancyContext context)
        //{
        //    string responseBody = backendResponse.Content.ReadAsStringAsync().Result;
        //    var response = (Nancy.Response)responseBody;
        //    response.StatusCode = (Nancy.HttpStatusCode)backendResponse.StatusCode;
        //    foreach (var header in backendResponse.Headers)
        //    {
        //        foreach (var value in header.Value)
        //            response.Headers.Add(header.Key, value);
        //    }
        //    foreach (var header in backendResponse.Content.Headers)
        //    {
        //        foreach (var value in header.Value)
        //            response.Headers.Add(header.Key, value);
        //    }
        //    return response;
        //}
    }
}
