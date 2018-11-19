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

            //JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ContractResolver = new DictionaryAsArrayResolver();
            string json = JsonConvert.SerializeObject(InputSchema);

            string response = ApiRequest(Server, Token, "POST", "string", "/grasshopper", json);

            Schema output = JsonConvert.DeserializeObject<Schema>(response);
            return output;
            //Task<Schema> task = Request(InputSchema, Server, Token);
            //var result = task.Result;
            //return result;
        }


        public static dynamic ApiRequest(string server, string token, string requestType, string responseType, string urlExt, string jsonArguments = null) {


            string requestUrl = server + urlExt;

            var bytes = Encoding.Default.GetBytes(jsonArguments);

            using (var client = new System.Net.WebClient()) {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Authorization", token);

                //client.Headers.Add("Cache-Control", "no-cache");
                //client.Headers.Add("Host", "35.231.149.199");
                //client.Headers.Add("POST", "/sizing/bay/concrete/flatplate HTTP/1.1");

                var responseString = "";
                dynamic responseJson = null;

                try {
                    if (requestType == "POST") {
                        var response = client.UploadData(requestUrl, "POST", bytes);
                        responseString = Encoding.Default.GetString(response);
                    } else if (requestType == "GET") {
                        responseString = client.DownloadString(requestUrl);
                    }
                    responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseString);
                } catch (Exception e) {
                    string s = e.ToString();
                    // do nothing
                }
                if (responseType == "string") {
                    return responseString;
                } else {
                    return responseJson;
                }
            }
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
                    //JsonSerializerSettings settings = new JsonSerializerSettings();
                    //settings.ContractResolver = new DictionaryAsArrayResolver();
                    request.Content = new StringContent(JsonConvert.SerializeObject(InputSchema), Encoding.UTF8, "application/json");

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
