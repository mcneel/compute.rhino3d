using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resthopper.IO;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Resthopper.Core
{
    public class ResthopperPipeline
    {
        public static Schema Request(Schema InputSchema, string server, string token)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"{server}/grasshopper");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{server}/grasshopper");
            request.Content = new StringContent(JsonConvert.SerializeObject(InputSchema), Encoding.UTF8, "application/json");

            Schema output = null;
            HttpResponseMessage result = new HttpResponseMessage();
            client.SendAsync(request).ContinueWith(responseTask =>
            {
                result = responseTask.Result;
                output = JsonConvert.DeserializeObject<Schema>(result.Content.ToString());
                client.Dispose();
            });
            return output;
        }
    }
}
