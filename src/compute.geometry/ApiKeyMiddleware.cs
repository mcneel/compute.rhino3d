using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace compute.geometry
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "RhinoComputeKey";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // GET and OPTIONS are exempt: matches the original Nancy design and
            // accommodates rhino.compute's reverse proxy which only forwards the
            // RhinoComputeKey header on POST. POST is the auth-sensitive path
            // (/grasshopper, /io).
            var method = context.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsOptions(method))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                Log.Warning("401 rejecting {Method} {Path}: missing {HeaderName} header",
                    method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Requires {APIKEYNAME} header");
                return;
            }

            if (!string.Equals(extractedApiKey.ToString(), Config.ApiKey, StringComparison.Ordinal))
            {
                Log.Warning("401 rejecting {Method} {Path}: {HeaderName} header does not match server's configured key",
                    method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
