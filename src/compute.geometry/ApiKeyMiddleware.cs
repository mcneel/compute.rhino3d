using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace compute.geometry
{
    // Native ASP.NET Core middleware that requires the RhinoComputeKey header on
    // every request when Config.ApiKey is non-empty. Mirrors the rhino.compute
    // ApiKeyMiddleware so the rhino.compute → compute.geometry proxy chain works
    // end-to-end (rhino.compute's ReverseProxy already forwards this header).
    //
    // Wired in Startup.Configure conditionally on Config.ApiKey, matching the
    // rhino.compute pattern — a missing/empty key disables the middleware so
    // existing deployments that don't set RHINO_COMPUTE_KEY are unaffected.
    //
    // GET and OPTIONS are exempted to match the historical Nancy-era behavior
    // (informational endpoints like /version, /sdk, /healthcheck are free) and
    // because the rhino.compute reverse proxy only forwards the RhinoComputeKey
    // header on POST requests (ReverseProxy.SendProxyRequest GET branch passes
    // no headers). Auth-sensitive work happens on POST (e.g. /grasshopper).
    public class ApiKeyMiddleware
    {
        const string API_KEY_NAME = "RhinoComputeKey";
        readonly RequestDelegate next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // GET is informational; OPTIONS is CORS preflight. Both pass through.
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
