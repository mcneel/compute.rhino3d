using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Compute.Http
{
    /// <summary>
    /// HTTP compression shared by rhino.compute and compute.geometry, on unless
    /// RHINO_COMPUTE_COMPRESSION=false: only gzip request bodies are accepted, and answers of at least
    /// <see cref="CompressionMiddleware.MinimumBytes"/> are gzipped for clients that send Accept-Encoding: gzip.
    /// </summary>
    public static class HttpCompression
    {
        public static IServiceCollection AddHttpCompression(this IServiceCollection services)
            => services.AddRequestDecompression(options =>
            {
                // ASP.NET Core also decodes Brotli and deflate by default; Brotli decoding was CVE-2026-111497.
                foreach (string encoding in options.DecompressionProviders.Keys.ToList())
                {
                    if (!string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase))
                        options.DecompressionProviders.Remove(encoding);
                }
            });

        public static IApplicationBuilder UseHttpCompression(this IApplicationBuilder app)
            => app.UseRequestDecompression().UseMiddleware<CompressionMiddleware>();
    }

    public sealed class CompressionMiddleware
    {
        /// <summary>Smaller answers gain nothing from gzip and can even grow.</summary>
        public const int MinimumBytes = 1024;

        readonly RequestDelegate next;

        public CompressionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // RFC 7694: tells clients this server accepts gzip request bodies.
            context.Response.Headers[HeaderNames.AcceptEncoding] = "gzip";

            if (!AcceptsGzip(context.Request.Headers[HeaderNames.AcceptEncoding]))
            {
                await next(context);
                return;
            }

            // Endpoints build their answers in memory anyway, so holding the answer here costs
            // little and lets small answers go out uncompressed.
            Stream body = context.Response.Body;
            using var answer = new MemoryStream();
            context.Response.Body = answer;
            try
            {
                await next(context);
            }
            finally
            {
                context.Response.Body = body;
            }

            context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.AcceptEncoding);
            if (answer.Length == 0)
                return;

            if (answer.Length >= MinimumBytes && !context.Response.Headers.ContainsKey(HeaderNames.ContentEncoding))
            {
                using var compressed = new MemoryStream();
                using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                    answer.WriteTo(gzip);
                context.Response.Headers[HeaderNames.ContentEncoding] = "gzip";
                context.Response.ContentLength = compressed.Length;
                compressed.Position = 0;
                await compressed.CopyToAsync(body, context.RequestAborted);
                return;
            }

            context.Response.ContentLength = answer.Length;
            answer.Position = 0;
            await answer.CopyToAsync(body, context.RequestAborted);
        }

        /// <summary>True when the client lists gzip (or *) with a non-zero quality, and doesn't refuse gzip explicitly.</summary>
        public static bool AcceptsGzip(StringValues acceptEncoding)
        {
            if (!StringWithQualityHeaderValue.TryParseList(acceptEncoding, out var codings))
                return false;

            double? gzip = null;
            double? any = null;
            foreach (var coding in codings)
            {
                if (StringSegment.Equals(coding.Value, "gzip", StringComparison.OrdinalIgnoreCase))
                    gzip = coding.Quality ?? 1;
                else if (coding.Value == "*")
                    any = coding.Quality ?? 1;
            }
            return (gzip ?? any ?? 0) > 0;
        }
    }
}
