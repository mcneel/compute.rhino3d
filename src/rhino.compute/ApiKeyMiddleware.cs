using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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
                Log.Warning("401 rejecting {Method} {Path}: missing {HeaderName} header",
                    context.Request.Method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided.");
                return;
            }

            var apiKey = Config.ApiKey;

            if (!apiKey.Equals(extractedApiKey))
            {
                Log.Warning("401 rejecting {Method} {Path}: {HeaderName} header does not match server's configured key",
                    context.Request.Method, context.Request.Path, APIKEYNAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
