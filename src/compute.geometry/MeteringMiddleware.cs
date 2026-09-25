using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace compute.geometry
{
    /// <summary>
    /// Measures each request's body sizes, CPU time (including processes compute.geometry starts) and wall
    /// time, and attributes it to a client. Reports it in response headers and/or the usage log. CPU is exact
    /// only when this process handles one request at a time (as /grasshopper does).
    /// </summary>
    public class MeteringMiddleware
    {
        public const string INGRESS_BYTES_HEADER = "Rhino-Compute-Ingress-Bytes";
        public const string EGRESS_BYTES_HEADER = "Rhino-Compute-Egress-Bytes";
        public const string CPU_SECONDS_HEADER = "Rhino-Compute-Cpu-Seconds";
        public const string PID_HEADER = "Rhino-Compute-Pid";
        public const string CLIENT_HEADER = "Rhino-Compute-Client";
        const int MAX_CLIENT_LENGTH = 128;

        public static readonly string[] Headers = { INGRESS_BYTES_HEADER, EGRESS_BYTES_HEADER, CPU_SECONDS_HEADER, PID_HEADER };

        private readonly RequestDelegate next;
        private readonly string defaultClient;

        public MeteringMiddleware(RequestDelegate next)
        {
            this.next = next;
            ProcessTreeCpu.Initialize();
            defaultClient = string.IsNullOrEmpty(Config.ApiKey) ? "default" : "key:" + KeyFingerprint(Config.ApiKey);
        }

        // The client a gateway named in Rhino-Compute-Client, else the API key's fingerprint, else "default".
        string ClientId(HttpContext context)
        {
            string client = context.Request.Headers[CLIENT_HEADER].ToString().Trim();
            if (client.Length == 0)
                return defaultClient;
            return client.Length > MAX_CLIENT_LENGTH ? client.Substring(0, MAX_CLIENT_LENGTH) : client;
        }

        // Never record the key itself.
        static string KeyFingerprint(string key) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0, 4).ToLowerInvariant();

        public async Task InvokeAsync(HttpContext context)
        {
            var startUtc = DateTime.UtcNow;
            long startTimestamp = Stopwatch.GetTimestamp();
            var cpuBefore = ProcessTreeCpu.Total();
            var originalRequestBody = context.Request.Body;
            var originalResponseBody = context.Response.Body;
            var requestBody = new CountingReadStream(originalRequestBody);

            // Buffered so the response size is known before the headers are sent.
            using var responseBuffer = new MemoryStream();
            context.Request.Body = requestBody;
            context.Response.Body = responseBuffer;
            try
            {
                await next(context);
            }
            finally
            {
                context.Request.Body = originalRequestBody;
                context.Response.Body = originalResponseBody;
            }

            var cpu = ProcessTreeCpu.Total() - cpuBefore;
            if (Config.MeteringHeaders)
            {
                context.Response.Headers[INGRESS_BYTES_HEADER] = requestBody.BytesRead.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers[EGRESS_BYTES_HEADER] = responseBuffer.Length.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers[CPU_SECONDS_HEADER] = cpu.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                context.Response.Headers[PID_HEADER] = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
            }
            // Health checks are infrastructure traffic, not client usage.
            if (UsageLog.Enabled && !context.Request.Path.Equals("/healthcheck", StringComparison.OrdinalIgnoreCase))
            {
                UsageLog.Write(startUtc, ClientId(context), context.Request.Method, context.Request.Path.Value, context.Response.StatusCode,
                    requestBody.BytesRead, responseBuffer.Length, cpu.TotalSeconds, Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
            }
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody, context.RequestAborted);
        }

        sealed class CountingReadStream : Stream
        {
            private readonly Stream inner;

            public CountingReadStream(Stream inner)
            {
                this.inner = inner;
            }

            public long BytesRead { get; private set; }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = inner.Read(buffer, offset, count);
                BytesRead += read;
                return read;
            }

            public override int Read(Span<byte> buffer)
            {
                int read = inner.Read(buffer);
                BytesRead += read;
                return read;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                int read = await inner.ReadAsync(buffer, offset, count, cancellationToken);
                BytesRead += read;
                return read;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                int read = await inner.ReadAsync(buffer, cancellationToken);
                BytesRead += read;
                return read;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
