using Nancy.Extensions;
using System.Net.Http;

namespace compute.frontend
{
    public class ProxyModule : Nancy.NancyModule
    {
        public ProxyModule()
        {
            int backendPort = Env.GetEnvironmentInt("COMPUTE_BACKEND_PORT", 8081);

            Get["/"] =
            Get["/{uri*}"] = _ =>
            {
                var proxy_url = GetProxyUrl((string)_.uri, backendPort, Context.Request.Query);
                var client = CreateProxyClient(Context);
                try
                {
                    var backendResponse = client.GetAsync(proxy_url).Result;
                    return CreateProxyResponse(backendResponse, Context);
                }
                catch
                {
                    return new Nancy.Responses.TextResponse(Nancy.HttpStatusCode.InternalServerError, "Backend not available");
                }
            };

            Post["/"] =
            Post["/{uri*}"] = _ =>
            {
                var proxy_url = GetProxyUrl((string)_.uri, backendPort, Context.Request.Query);
                var client = CreateProxyClient(Context);
                object o_content;
                StringContent content;
                if (Context.Items.TryGetValue("request-body", out o_content))
                    content = new StringContent(o_content as string);
                else
                    content = new StringContent(Context.Request.Body.AsString());

                try
                {
                    var backendResponse = client.PostAsync(proxy_url, content).Result;
                    return CreateProxyResponse(backendResponse, Context);
                }
                catch
                {
                    return 500;
                }
            };
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
                if (header.Key == "Content-Length")
                    continue;
                if (header.Key == "Content-Type")
                    continue;
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            client.DefaultRequestHeaders.Add("X-Compute-Id", (string)Context.Items["RequestId"]);
            client.DefaultRequestHeaders.Add("X-Compute-Host", (string)Context.Items["Hostname"]);
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
