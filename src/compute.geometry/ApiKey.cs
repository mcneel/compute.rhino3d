using System;
using System.Collections.Generic;

namespace compute.geometry
{
    static class ApiKey
    {
        static string _apiKey;
        public static void Initialize(Nancy.Bootstrapper.IPipelines pipelines)
        {
            _apiKey = System.Environment.GetEnvironmentVariable("RHINO_COMPUTE_KEY");
            // If no key has been set as an environment variable, then limiting
            // requests based on a key is disabled
            if (string.IsNullOrWhiteSpace(_apiKey))
                return;

            pipelines.BeforeRequest += CheckApiKey;
        }

        static Nancy.Response CheckApiKey(Nancy.NancyContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "OPTIONS")
                return null; // GET and OPTIONS requests are free

            var requestIds = new List<string>(context.Request.Headers["RhinoComputeKey"]);
            if (requestIds.Count != 1)
                return NoKeyResponse();
            var key_in_header = requestIds[0];
            if (string.Equals(key_in_header, _apiKey, StringComparison.Ordinal))
                return null;

            return NoKeyResponse();
        }

        private static Nancy.Response NoKeyResponse()
        {
            var response = (Nancy.Response)"Requires rhino_compute_key header";
            response.StatusCode = Nancy.HttpStatusCode.Unauthorized;
            return response;
        }
    }
}
