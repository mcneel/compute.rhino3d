using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class ResthopperPipeline
    {
        public static async Task<Schema> Request(Schema InputSchema, string server, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{server}/grasshopper");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{server}/grasshopper");
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ContractResolver = new DictionaryAsArrayResolver();
                request.Content = new StringContent(JsonConvert.SerializeObject(InputSchema, settings), Encoding.UTF8, "application/json");

                Schema output = null;
                using (HttpResponseMessage result = await client.SendAsync(request))
                using (HttpContent content = result.Content)
                {
                    var data = content.ReadAsStringAsync().Result;
                    output = JsonConvert.DeserializeObject<Schema>(result.Content.ToString());
                }
               
                return output;
            }
        }
    }
}
