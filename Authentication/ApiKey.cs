using System;
using Nancy;
using Nancy.Bootstrapper;

namespace RhinoCommon.Rest.Authentication
{
    public static class ApiKeyAuth
    {
        public static void AddAuthApiKey(this IPipelines pipelines)
        {
            pipelines.BeforeRequest += VerifyApiKey;
            Console.WriteLine("RhinoAccounts autnentication enabled");
        }

        private static Response VerifyApiKey(NancyContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "OPTIONS")
                return null; // GET and OPTIONS requests are free :)

            var requestIds = new System.Collections.Generic.List<string>(context.Request.Headers["api_token"]);
            if (requestIds.Count != 1)
                return NotAuthenticatedResponse();
            var api_token = requestIds[0];

            if (string.IsNullOrWhiteSpace(api_token))
                return NotAuthenticatedResponse();
            if (api_token.Length > 2 && api_token.Contains("@"))
            {
                context.Items["auth_user"] = api_token;
                return null;
            }
            
            return NotAuthenticatedResponse();
        }

        private static Response NotAuthenticatedResponse()
        {
            var response = (Response)"Requires api_token header with your email address";
            response.StatusCode = Nancy.HttpStatusCode.Unauthorized;
            return response;
        }
    }
}
