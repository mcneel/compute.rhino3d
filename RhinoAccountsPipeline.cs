namespace RhinoCommon.Rest
{
    using System;
    using System.Net;
    using Nancy;
    using Nancy.Bootstrapper;

    public static class RhinoAccountsPipeline
    {
        public static void AddRhinoAccountsAuth(this IPipelines pipelines)
        {
            pipelines.BeforeRequest += VerifyRhinoAccount;
            Console.WriteLine("RhinoAccounts autnentication enabled");
        }
        private static Response VerifyRhinoAccount(NancyContext context)
        {
            var authHeader = context.Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(authHeader))
                return NotAuthenticatedResponse();

            // Verify token information
            WebClient client = new WebClient();
            client.Headers["Authorization"] = authHeader;
            string body;
            try
            {
                body = client.DownloadString("https://accounts.rhino3d.com/oauth2/tokeninfo");
                var json = Newtonsoft.Json.Linq.JObject.Parse(body);
                var audience = (string)json["audience"];
                if (string.IsNullOrWhiteSpace(audience) || audience != "compute")
                    return NotAuthenticatedResponse();
            }
            catch
            {
                return NotAuthenticatedResponse();
            }


            return null;
        }

        private static Response NotAuthenticatedResponse()
        {
            var response = (Response)"Requires Rhino Accounts token via Bearer Authorization. Get a token at https://www.rhino3d.com/compute/login";
            response.StatusCode = Nancy.HttpStatusCode.Unauthorized;
            return response;
        }
    }
}
