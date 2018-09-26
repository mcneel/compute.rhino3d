using System;
using System.Net;
using Nancy;
using Nancy.Bootstrapper;
using Serilog;

namespace compute.frontend.Authentication
{
    public static class RhinoAccounts
    {
        public static void AddAuthRhinoAccount(this IPipelines pipelines)
        {
            pipelines.BeforeRequest += VerifyRhinoAccount;
            Log.Information("RhinoAccounts authentication enabled");
        }
        private static Response VerifyRhinoAccount(NancyContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "OPTIONS")
                return null; // GET and OPTIONS requests are free :)

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

                body = client.DownloadString("https://accounts.rhino3d.com/oauth2/userinfo");
                json = Newtonsoft.Json.Linq.JObject.Parse(body);
                var email = (string)json["email"];
                context.Items["auth_user"] = email;
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
