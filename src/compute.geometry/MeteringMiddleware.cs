using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace compute.geometry
{
    // Endpoint metadata marking client work that is billed; every other request is overhead.
    public sealed class BillableEndpoint
    {
        public static readonly BillableEndpoint Instance = new BillableEndpoint();
    }

    public static class BillableEndpointExtensions
    {
        public static TBuilder Billable<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder =>
            builder.WithMetadata(BillableEndpoint.Instance);
    }

    // Runs after the API key check, so rejected requests are never billed. Billable requests are received in
    // full before they count as running, so a slow upload doesn't take a share of other requests' CPU.
    public class BillableMiddleware
    {
        const int REQUEST_MEMORY_BUFFER = 4 * 1024 * 1024;

        private readonly RequestDelegate next;

        public BillableMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<BillableEndpoint>() == null)
            {
                await next(context);
                return;
            }
            try
            {
                context.Request.EnableBuffering(REQUEST_MEMORY_BUFFER);
                await context.Request.Body.DrainAsync(context.RequestAborted);
                context.Request.Body.Position = 0;
                using (CpuLedger.EnterBillable(RequestClients.For(context)))
                    await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
            }
        }
    }

    // Who a billable request is for. A nested call, opened by this process or one it started while serving a
    // request, is for that request's client. Otherwise Rhino-Compute-Client names the client: trusted from a
    // gateway when compute.geometry runs on its own, and only from rhino.compute when it is one of its children.
    // Without it, the API key's fingerprint (never the key), else "default".
    static class RequestClients
    {
        public const string CLIENT_HEADER = "Rhino-Compute-Client";
        const int MAX_CLIENT_LENGTH = 128;

        static readonly int[] self = { Environment.ProcessId };
        static readonly Lazy<string> defaultClient = new Lazy<string>(() =>
            string.IsNullOrEmpty(Config.ApiKey) ? "default" : "key:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Config.ApiKey)), 0, 4).ToLowerInvariant());

        enum Origin { Parent, Self, Other }

        static readonly object ORIGIN = new object();

        public static string For(HttpContext context)
        {
            bool child = Shutdown.ParentProcesses?.Count > 0;
            switch (OriginOf(context))
            {
                case Origin.Parent:
                    return FromHeader(context);
                case Origin.Self:
                    return CpuLedger.OldestBillableClient() ?? defaultClient.Value;
                default:
                    return child ? defaultClient.Value : FromHeader(context);
            }
        }

        // Which process opened a connection can't change, so it's looked up once per connection.
        static Origin OriginOf(HttpContext context)
        {
            var items = context.Features.Get<Microsoft.AspNetCore.Connections.Features.IConnectionItemsFeature>()?.Items;
            if (items != null && items.TryGetValue(ORIGIN, out object cached))
                return (Origin)cached;

            var origin = Origin.Other;
            var connection = context.Connection;
            if (LocalConnections.FromThisMachine(connection.RemoteIpAddress))
            {
                int[] parents = Shutdown.ParentProcesses?.Keys.ToArray() ?? Array.Empty<int>();
                if (parents.Length > 0 && LocalConnections.OpenedBy(connection.RemotePort, connection.LocalPort, parents, orStartedBy: false) != null)
                    origin = Origin.Parent;
                else if (LocalConnections.OpenedBy(connection.RemotePort, connection.LocalPort, self, orStartedBy: true) != null)
                    origin = Origin.Self;
            }
            if (items != null)
                items[ORIGIN] = origin;
            return origin;
        }

        static string FromHeader(HttpContext context)
        {
            string client = context.Request.Headers[CLIENT_HEADER].ToString().Trim();
            if (client.Length == 0)
                return defaultClient.Value;
            return client.Length > MAX_CLIENT_LENGTH ? client.Substring(0, MAX_CLIENT_LENGTH) : client;
        }
    }

    /// <summary>
    /// Measures each request's body sizes, CPU time (including processes compute.geometry starts, divided
    /// between requests by <see cref="CpuLedger"/>) and wall time, and attributes billable requests to a
    /// client. Reports it in response headers and/or the usage log.
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
            CpuLedger.Start();
            UsageLog.Start();
        }

        // How non-billable requests are grouped in the usage log's overhead records.
        static string OverheadLabel(HttpContext context)
        {
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                return "rejected";
            if (HttpMethods.IsOptions(context.Request.Method))
                return "preflight";
            if (context.GetEndpoint() is RouteEndpoint endpoint)
                return $"{context.Request.Method} /{endpoint.RoutePattern.RawText?.TrimStart('/')}";
            return "not found";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startUtc = DateTime.UtcNow;
            long startTimestamp = Stopwatch.GetTimestamp();
            var cpu = CpuLedger.Begin();
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
                CpuLedger.End(cpu, cpu.Billable ? null : OverheadLabel(context));
            }

            if (Config.MeteringHeaders)
            {
                context.Response.Headers[INGRESS_BYTES_HEADER] = requestBody.BytesRead.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers[EGRESS_BYTES_HEADER] = responseBuffer.Length.ToString(CultureInfo.InvariantCulture);
                if (cpu.Billable)
                    context.Response.Headers[CPU_SECONDS_HEADER] = cpu.CpuSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                context.Response.Headers[PID_HEADER] = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
            }
            if (cpu.Billable && UsageLog.Enabled)
            {
                UsageLog.WriteRequest(startUtc, cpu.Client, context.Request.Method, context.Request.Path.Value, context.Response.StatusCode,
                    requestBody.BytesRead, responseBuffer.Length, cpu, Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
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
