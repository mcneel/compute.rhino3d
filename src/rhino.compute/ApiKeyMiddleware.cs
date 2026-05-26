using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading.Tasks;

namespace rhino.compute
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
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                Log.Warning("401 rejected {Method} {Path}: missing {HeaderName} header",
                    context.Request.Method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided.");
                return;
            }

            var apiKey = Config.ApiKey;

            if (!string.Equals(extractedApiKey.ToString(), apiKey, StringComparison.Ordinal))
            {
                Log.Warning("401 rejected {Method} {Path}: {HeaderName} header does not match server's configured key",
                    context.Request.Method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
