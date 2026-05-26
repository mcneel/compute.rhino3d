using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Cryptography;
using System.Text;
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

            // Timing-safe comparison. String.Equals exits early on the first byte that
            // differs, which leaks key-prefix information to an attacker measuring response
            // times across many requests. FixedTimeEquals walks both buffers fully every
            // time, so the per-byte work is constant regardless of where they differ.
            // (Length difference still short-circuits — that's not a meaningful leak.)
            var providedBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());
            var configuredBytes = Encoding.UTF8.GetBytes(Config.ApiKey);
            if (!CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes))
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
