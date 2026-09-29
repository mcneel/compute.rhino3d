using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
            if (!CpuLedger.IsMetered || context.GetEndpoint()?.Metadata.GetMetadata<BillableEndpoint>() == null)
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

    // The definition a billable request used, set by its endpoint for its usage record. Cached is whether a
    // solve came from the solve cache (null for requests that don't solve).
    static class RequestDefinitions
    {
        public sealed record Use(string Id, string Name, bool? Cached);

        const int MAX_NAME_LENGTH = 128;
        static readonly object KEY = new object();

        public static void Set(HttpContext context, GrasshopperDefinition definition, string fileName, bool? cached)
        {
            if (CpuLedger.IsMetered && definition?.Id != null)
                context.Items[KEY] = new Use(definition.Id, CleanName(fileName) ?? CleanName(definition.Name), cached);
        }

        public static Use Get(HttpContext context) => context.Items.TryGetValue(KEY, out object use) ? use as Use : null;

        // Clients may send a path; only its last part is kept.
        static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            name = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
            name = name.Substring(name.LastIndexOfAny(new[] { '/', '\\' }) + 1);
            if (name.Length == 0)
                return null;
            return name.Length > MAX_NAME_LENGTH ? name.Substring(0, MAX_NAME_LENGTH) : name;
        }
    }

    // What went wrong with a billable request, for its usage record: why it failed, and the Grasshopper errors and
    // warnings of a solve or of a definition /io loaded, the first MAX_MESSAGES of each with how many there were. Messages can hold paths and input values,
    // so each is kept to one short line.
    static class RequestOutcome
    {
        public sealed record Result(string Error, string[] Errors, string[] Warnings, int? ErrorCount, int? WarningCount);

        const int MAX_MESSAGE_LENGTH = 300;
        const int MAX_MESSAGES = 20;
        const int MAX_BODY_READ = 64 * 1024;
        static readonly object KEY = new object();

        sealed class Notes
        {
            public string Error;
            public string[] Errors, Warnings;
            public int? ErrorCount, WarningCount;
        }

        static Notes For(HttpContext context)
        {
            if (!context.Items.TryGetValue(KEY, out object notes))
                context.Items[KEY] = notes = new Notes();
            return (Notes)notes;
        }

        public static void Failed(HttpContext context, string message)
        {
            if (CpuLedger.IsMetered)
                For(context).Error ??= Clean(message);
        }

        public static void Reported(HttpContext context, IList<string> errors, IList<string> warnings)
        {
            if (!CpuLedger.IsMetered)
                return;
            var notes = For(context);
            notes.ErrorCount = errors?.Count > 0 ? errors.Count : null;
            notes.WarningCount = warnings?.Count > 0 ? warnings.Count : null;
            notes.Errors = First(errors);
            notes.Warnings = First(warnings);
            if (notes.Errors != null)
                notes.Error ??= notes.Errors[0];
        }

        static string[] First(IList<string> messages)
        {
            var kept = messages?.Select(Clean).Where(m => m != null).Take(MAX_MESSAGES).ToArray();
            return kept?.Length > 0 ? kept : null;
        }

        // A failed request says why in what was noted while it ran, else in its response: a JSON message, errors or
        // error, or the first line of text.
        public static Result Get(HttpContext context, MemoryStream body)
        {
            var notes = context.Items.TryGetValue(KEY, out object value) ? value as Notes : null;
            bool failed = context.Response.StatusCode >= 400;
            string error = failed ? notes?.Error ?? FromBody(body, context.Response.ContentType) : null;
            if (notes == null && error == null)
                return null;
            return new Result(error, notes?.Errors, notes?.Warnings, notes?.ErrorCount, notes?.WarningCount);
        }

        static string FromBody(MemoryStream body, string contentType)
        {
            if (body.Length == 0)
                return null;
            string text = Encoding.UTF8.GetString(body.GetBuffer(), 0, (int)Math.Min(body.Length, MAX_BODY_READ));
            if (contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true || text.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var json = JsonDocument.Parse(text);
                    if (json.RootElement.ValueKind != JsonValueKind.Object)
                        return null;
                    foreach (string name in new[] { "message", "detail", "errors", "error", "title" })
                    {
                        if (!json.RootElement.TryGetProperty(name, out var property))
                            continue;
                        if (property.ValueKind == JsonValueKind.String && Clean(property.GetString()) is string message)
                            return message;
                        if (property.ValueKind == JsonValueKind.Array && property.GetArrayLength() > 0 && property[0].ValueKind == JsonValueKind.String)
                            return Clean(property[0].GetString());
                    }
                    return null;
                }
                catch (JsonException)
                {
                }
            }
            return Clean(text.Split('\n').FirstOrDefault(line => line.Trim().Length > 0));
        }

        static string Clean(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;
            string line = string.Join(" ", message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()));
            line = new string(line.Where(c => !char.IsControl(c)).ToArray()).Trim();
            if (line.Length == 0)
                return null;
            return line.Length > MAX_MESSAGE_LENGTH ? line.Substring(0, MAX_MESSAGE_LENGTH - 1) + "…" : line;
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
        public const string REQUEST_ID_HEADER = "Rhino-Compute-Request-Id";
        const int MAX_REQUEST_ID_LENGTH = 128;

        public static readonly string[] Headers = { INGRESS_BYTES_HEADER, EGRESS_BYTES_HEADER, CPU_SECONDS_HEADER, PID_HEADER, REQUEST_ID_HEADER };

        private readonly RequestDelegate next;

        public MeteringMiddleware(RequestDelegate next)
        {
            this.next = next;
            if (Config.MeteringHeaders || !string.IsNullOrEmpty(Config.UsageLogPath))
                CpuLedger.Start();
            if (!string.IsNullOrEmpty(Config.UsageLogPath))
                UsageLog.Start();
            AgentLink.Start();
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

        // The caller's own ID when it sends one, so its records match its logs; otherwise a new one.
        static string RequestId(HttpContext context)
        {
            string id = context.Request.Headers[REQUEST_ID_HEADER].ToString().Trim();
            if (id.Length > 0 && id.Length <= MAX_REQUEST_ID_LENGTH && id.All(c => c >= ' ' && c <= '~'))
                return id;
            return Guid.NewGuid().ToString("N");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!CpuLedger.Started)
            {
                await next(context);
                return;
            }
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

            string requestId = cpu.Billable ? RequestId(context) : null;
            if (Config.MeteringHeaders)
            {
                context.Response.Headers[INGRESS_BYTES_HEADER] = requestBody.BytesRead.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers[EGRESS_BYTES_HEADER] = responseBuffer.Length.ToString(CultureInfo.InvariantCulture);
                if (cpu.Billable)
                {
                    context.Response.Headers[CPU_SECONDS_HEADER] = cpu.CpuSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                    context.Response.Headers[REQUEST_ID_HEADER] = requestId;
                }
                context.Response.Headers[PID_HEADER] = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
            }
            if (cpu.Billable && UsageLog.Recording)
            {
                UsageLog.WriteRequest(startUtc, requestId, cpu.Client, context.Request.Method, context.Request.Path.Value,
                    context.Response.StatusCode, RequestDefinitions.Get(context), RequestOutcome.Get(context, responseBuffer), requestBody.BytesRead,
                    responseBuffer.Length, cpu,
                    Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
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
