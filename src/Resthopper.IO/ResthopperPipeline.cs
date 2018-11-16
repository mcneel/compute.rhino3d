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

    public static class Utils
    {
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }


    public class ResthopperPipeline
    {
        public static string Token { get; set; }
        public static string Server { get; set; }

        public static Schema Request(Schema InputSchema) {

            Task<Schema> task = Request(InputSchema, Server, Token);
            var result = task.Result;
            return result;
        }

        public static async Task<Schema> Request(Schema InputSchema, string server, string token)
        {
            try {
                using (HttpClient client = new HttpClient()) {
                    string endPoint = "grasshopper";
                    client.BaseAddress = new Uri(string.Format("{0}/{1}", server, endPoint));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{server}/grasshopper");
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.ContractResolver = new DictionaryAsArrayResolver();
                    request.Content = new StringContent(JsonConvert.SerializeObject(InputSchema, settings), Encoding.UTF8, "application/json");

                    Schema output = null;
                    using (HttpResponseMessage result = await client.SendAsync(request))
                    using (HttpContent content = result.Content) {
                        var data = content.ReadAsStringAsync().Result;
                        output = JsonConvert.DeserializeObject<Schema>(result.Content.ToString());
                    }

                    return output;
                }
            } catch (Exception e) {

                throw;
            }
        }
    }
}
