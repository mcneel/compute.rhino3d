using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace compute.geometry
{
    /// <summary>
    /// Adds the request and response body sizes, in bytes, the CPU time the request used, and the id of
    /// the serving process to every response. CPU includes processes compute.geometry starts; it is exact
    /// only when this process handles one request at a time (as /grasshopper does).
    /// </summary>
    public class MeteringMiddleware
    {
        public const string INGRESS_BYTES_HEADER = "Rhino-Compute-Ingress-Bytes";
        public const string EGRESS_BYTES_HEADER = "Rhino-Compute-Egress-Bytes";
        public const string CPU_SECONDS_HEADER = "Rhino-Compute-Cpu-Seconds";
        public const string PID_HEADER = "Rhino-Compute-Pid";

        public static readonly string[] Headers = { INGRESS_BYTES_HEADER, EGRESS_BYTES_HEADER, CPU_SECONDS_HEADER, PID_HEADER };

        private readonly RequestDelegate next;

        public MeteringMiddleware(RequestDelegate next)
        {
            this.next = next;
            ProcessTreeCpu.Initialize();
        }

        public async Task InvokeAsync(HttpContext context)
        {
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
            context.Response.Headers[INGRESS_BYTES_HEADER] = requestBody.BytesRead.ToString(CultureInfo.InvariantCulture);
            context.Response.Headers[EGRESS_BYTES_HEADER] = responseBuffer.Length.ToString(CultureInfo.InvariantCulture);
            context.Response.Headers[CPU_SECONDS_HEADER] = cpu.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
            context.Response.Headers[PID_HEADER] = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
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
