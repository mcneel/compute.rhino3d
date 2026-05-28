using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace compute.geometry
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate next;
        private const string API_KEY_NAME = "RhinoComputeKey";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            this.next = next;
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
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_NAME, out var extractedApiKey))
            {
                Log.Warning("401 rejecting {Method} {Path}: missing {HeaderName} header",
                    method, context.Request.Path, API_KEY_NAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Requires {API_KEY_NAME} header");
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
                Log.Warning("401 rejecting {Method} {Path}: {HeaderName} header does not match server's configured key",
                    method, context.Request.Path, API_KEY_NAME);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await next(context);
        }
    }
}
