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
            pipelines.BeforeRequest.AddItemToStartOfPipeline(VerifyRhinoAccount);
            Log.Information("RhinoAccounts authentication enabled");
        }
        private static Response VerifyRhinoAccount(NancyContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "OPTIONS")
                return null; // GET and OPTIONS requests are free :)

            var authHeader = context.Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(authHeader))
                return NotAuthenticatedResponse();

            try
            {
                const double ALLOWED_AUTHORIZED_SPAN_MINUTES = 5;
                Account acct;
                if (_accountCache.TryGetValue(authHeader, out acct) && acct != null)
                {
                    var span = DateTime.Now - acct.LastChecked;
                    if (span.TotalMinutes < ALLOWED_AUTHORIZED_SPAN_MINUTES && !string.IsNullOrWhiteSpace(acct.Email))
                    {
                        context.Items["auth_user"] = acct.Email;
                        return null;
                    }
                }
            }
            catch
            {
                // allow the code to continue if an exception occurs here
            }

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

                // Just clear the entire cache once we hit a limit. It won't take
                // a whole lot to rebuild the cache.
                const int ACCOUNT_CACHE_LIMIT = 1000;
                if (_accountCache.Count > ACCOUNT_CACHE_LIMIT)
                    _accountCache.Clear();

                _accountCache[authHeader] = new Account(email);

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

        class Account
        {
            public Account(string email)
            {
                Email = email;
                LastChecked = DateTime.Now;
            }
            public string Email { get; private set; }
            public DateTime LastChecked { get; private set; }
        }
        static System.Collections.Concurrent.ConcurrentDictionary<string, Account> _accountCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Account>();
    }
}
