using Nancy;
using Nancy.Extensions;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace compute.frontend
{
    public class ProxyModule : Nancy.NancyModule
    {
        private readonly string[] _skipHeaders = new string[] { "Content-Length", "Content-Type", "Host" };

        public ProxyModule()
        {
            int backendPort = Env.GetEnvironmentInt("COMPUTE_BACKEND_PORT", 8081);

            Get["/_debug"] = _ => "Hello World!"; // test frontend

            Get["/"] =
            Get["/{uri*}"] = _ =>
            {
                var proxy_url = GetProxyUrl((string)_.uri, backendPort, Context.Request.Query);
                var req = new HttpRequestMessage(HttpMethod.Get, proxy_url);

                return DoRequest(Context, req).Result;
            };

            Post["/"] =
            Post["/{uri*}"] = _ =>
            {
                var proxy_url = GetProxyUrl((string)_.uri, backendPort, Context.Request.Query);
                object o_content;
                StringContent content;
                if (Context.Items.TryGetValue("request-body", out o_content))
                    content = new StringContent(o_content as string);
                else
                    content = new StringContent(Context.Request.Body.AsString());
                var req = new HttpRequestMessage(HttpMethod.Post, proxy_url);
                req.Content = content;

                return DoRequest(Context, req).Result;
            };
        }

        private async Task<Response> DoRequest(NancyContext ctx, HttpRequestMessage req)
        {
            var client = CreateProxyClient(ctx);
            try
            {
                var backendResponse = await client.SendAsync(req);
                return CreateProxyResponse(backendResponse, ctx);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception occured while proxying request \"{RequestId}\" to the backend", Context.Items["RequestId"] as string);
                return new Nancy.Responses.TextResponse(Nancy.HttpStatusCode.InternalServerError, "{ \"message\": \"Backend not available\" }");
            }
        }

        string GetProxyUrl(string path, int backendPort, Nancy.DynamicDictionary querystring)
        {
            var qs = new System.Text.StringBuilder();

            foreach (string key in querystring.Keys)
            {
                if(qs.Length==0)
                    qs.Append($"?{key}");
                else
                    qs.Append($"&{key}");
                if (querystring[key].HasValue)
                    qs.Append("=").Append(querystring[key].Value as string);
            }
            if (string.IsNullOrWhiteSpace(path))
                return $"http://localhost:{backendPort}/{qs.ToString()}";

            return $"http://localhost:{backendPort}/{path}{qs.ToString()}";
        }

        HttpClient CreateProxyClient(Nancy.NancyContext context)
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            foreach (var header in Context.Request.Headers)
            {
                //Log.Debug("{key}: {value}", header.Key, header.Value);
                if (_skipHeaders.Contains(header.Key)) // host header causes 400 invalid host error
                    continue;
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            // client.DefaultRequestHeaders.Add("X-Forwarded-For", <UserHostAddress>); // TODO: see RequestLogEnricher.GetUserIPAddress()
            client.DefaultRequestHeaders.Add("X-Compute-Id", Context.Items["RequestId"] as string);
            client.DefaultRequestHeaders.Add("X-Compute-Host", Context.Items["Hostname"] as string);
            return client;
        }

        Nancy.Response CreateProxyResponse(HttpResponseMessage backendResponse, Nancy.NancyContext context)
        {
            string responseBody = backendResponse.Content.ReadAsStringAsync().Result;
            var response = (Nancy.Response)responseBody;
            response.StatusCode = (Nancy.HttpStatusCode)backendResponse.StatusCode;
            foreach (var header in backendResponse.Headers)
            {
                foreach (var value in header.Value)
                    response.Headers.Add(header.Key, value);
            }
            foreach (var header in backendResponse.Content.Headers)
            {
                foreach (var value in header.Value)
                    response.Headers.Add(header.Key, value);
            }
            return response;
        }
    }
}
