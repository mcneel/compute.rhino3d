using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compute.Http;
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
                // Inside compression, so these are the request as unpacked and the answer before it's packed.
                var usage = RequestUsage.For(context);
                var originalResponseBody = context.Response.Body;
                if (usage != null)
                {
                    usage.RequestBytes = context.Request.Body.Length;
                    usage.Response = new CountingWriteStream(originalResponseBody, RequestOutcome.MAX_BODY_READ);
                    context.Response.Body = usage.Response;
                }
                try
                {
                    using (CpuLedger.EnterBillable(RequestMetadata.For(context)))
                        await next(context);
                }
                finally
                {
                    context.Response.Body = originalResponseBody;
                }
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
            }
        }
    }

    // A billable request's sizes, measured by BillableMiddleware inside compression for its usage record.
    sealed class RequestUsage
    {
        static readonly object KEY = new object();

        public long RequestBytes;
        public CountingWriteStream Response;

        public static RequestUsage Begin(HttpContext context)
        {
            var usage = new RequestUsage();
            context.Items[KEY] = usage;
            return usage;
        }

        public static RequestUsage For(HttpContext context) => context.Items.TryGetValue(KEY, out object usage) ? usage as RequestUsage : null;
    }

    // Labels a caller attaches to a billable request in the Rhino-Compute-Metadata header, recorded as is in its usage
    // record for the caller's own use: a JSON object of up to MAX_KEYS string, number or true/false values. Anything
    // else is left out. A nested call, opened by this process or one it started while serving a request, carries that
    // request's metadata.
    static class RequestMetadata
    {
        public const string HEADER = "Rhino-Compute-Metadata";
        const int MAX_LENGTH = 4096;
        const int MAX_KEYS = 16;
        const int MAX_KEY_LENGTH = 40;
        const int MAX_VALUE_LENGTH = 500;

        static readonly int[] self = { Environment.ProcessId };
        static readonly object FROM_SELF = new object();

        public static JsonElement? For(HttpContext context) =>
            FromSelf(context) ? CpuLedger.OldestBillableMetadata() : Parse(context.Request.Headers[HEADER].ToString());

        public static JsonElement? Parse(string header)
        {
            if (string.IsNullOrWhiteSpace(header) || header.Length > MAX_LENGTH)
                return null;
            try
            {
                using var document = JsonDocument.Parse(header);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in root.EnumerateObject())
                {
                    if (names.Count == MAX_KEYS || property.Name.Length == 0 || property.Name.Length > MAX_KEY_LENGTH || !names.Add(property.Name))
                        return null;
                    var value = property.Value;
                    bool allowed = value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString().Length <= MAX_VALUE_LENGTH,
                        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => true,
                        _ => false,
                    };
                    if (!allowed)
                        return null;
                }
                return names.Count == 0 ? null : root.Clone();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // Which process opened a connection can't change, so it's looked up once per connection.
        static bool FromSelf(HttpContext context)
        {
            var items = context.Features.Get<Microsoft.AspNetCore.Connections.Features.IConnectionItemsFeature>()?.Items;
            if (items != null && items.TryGetValue(FROM_SELF, out object cached))
                return (bool)cached;
            var connection = context.Connection;
            bool fromSelf = LocalConnections.FromThisMachine(connection.RemoteIpAddress) &&
                LocalConnections.OpenedBy(connection.RemotePort, connection.LocalPort, self, orStartedBy: true) != null;
            if (items != null)
                items[FROM_SELF] = fromSelf;
            return fromSelf;
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

        // The start of a response kept for its error message.
        public const int MAX_BODY_READ = 64 * 1024;
        const int MAX_MESSAGE_LENGTH = 300;
        const int MAX_MESSAGES = 20;
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

        // A failed request says why in what was noted while it ran, else in its response (the start of it, uncompressed):
        // a JSON message, errors or error, or the first line of text.
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
    /// In the pipeline only while the usage log is on. Measures each request's body sizes, CPU time (including
    /// processes compute.geometry starts, divided between requests by <see cref="CpuLedger"/>) and wall time, and
    /// writes a usage record for each billable one.
    /// </summary>
    public class MeteringMiddleware
    {
        private readonly RequestDelegate next;

        public MeteringMiddleware(RequestDelegate next)
        {
            this.next = next;
            CpuLedger.Start();
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

        public async Task InvokeAsync(HttpContext context)
        {
            var startUtc = DateTime.UtcNow;
            long startTimestamp = Stopwatch.GetTimestamp();
            var cpu = CpuLedger.Begin();
            var usage = RequestUsage.Begin(context);
            // The request before compression unpacks it, and the answer as it's sent.
            string requestEncoding = HttpCompression.EncodingName(context.Request.Headers.ContentEncoding);
            var received = new CountingReadStream(context.Request.Body);
            context.Request.Body = received;
            var originalResponseBody = context.Response.Body;
            var sent = new CountingWriteStream(originalResponseBody, RequestOutcome.MAX_BODY_READ);
            context.Response.Body = sent;
            try
            {
                await next(context);
            }
            finally
            {
                context.Response.Body = originalResponseBody;
                CpuLedger.End(cpu, cpu.Billable ? null : OverheadLabel(context));
                // Also when the client left before the answer was sent: the work was done.
                if (cpu.Billable && UsageLog.Recording)
                {
                    string responseEncoding = HttpCompression.EncodingName(context.Response.Headers.ContentEncoding);
                    // An answer that wasn't packed, such as an error written after compression gave up, is as sent.
                    var answer = responseEncoding == "none" || usage.Response == null ? sent : usage.Response;
                    long requestBytes = received.BytesRead;
                    if (ClientRequest(context) is { } client)
                    {
                        requestEncoding = client.Encoding;
                        requestBytes = client.Bytes ?? usage.RequestBytes;
                    }
                    UsageLog.WriteRequest(startUtc, Guid.NewGuid().ToString("N"), cpu.Metadata, context.Request.Method, context.Request.Path.Value,
                        context.Response.StatusCode, RequestDefinitions.Get(context), RequestOutcome.Get(context, answer.Head),
                        usage.RequestBytes, requestBytes, requestEncoding, answer.BytesWritten, sent.BytesWritten, responseEncoding, cpu,
                        Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds);
                }
            }
        }

        // Behind rhino.compute, which unpacks the client's request itself, its note on the request as the client sent
        // it (see HttpCompression.CLIENT_REQUEST_HEADER). Only a child takes it.
        static (string Encoding, long? Bytes)? ClientRequest(HttpContext context)
        {
            string note = context.Request.Headers[HttpCompression.CLIENT_REQUEST_HEADER].ToString();
            if (note.Length == 0 || !(Shutdown.ParentProcesses?.Count > 0))
                return null;
            string encoding = "none";
            long? bytes = null;
            foreach (string part in note.Split(';'))
            {
                string[] pair = part.Split('=', 2);
                if (pair.Length != 2)
                    continue;
                string value = pair[1].Trim().ToLowerInvariant();
                if (pair[0].Trim() == "encoding" && value.Length > 0)
                    encoding = value;
                else if (pair[0].Trim() == "bytes" && long.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out long count))
                    bytes = count;
            }
            return (encoding, bytes);
        }
    }

    // Passes the response through as it's written, counting it and keeping its start for the error message.
    sealed class CountingWriteStream : Stream
    {
        private readonly Stream inner;
        private readonly int headLength;

        public CountingWriteStream(Stream inner, int headLength)
        {
            this.inner = inner;
            this.headLength = headLength;
        }

        public long BytesWritten { get; private set; }
        public MemoryStream Head { get; } = new MemoryStream();

        void Count(ReadOnlySpan<byte> buffer)
        {
            BytesWritten += buffer.Length;
            int keep = Math.Min(buffer.Length, headLength - (int)Head.Length);
            if (keep > 0)
                Head.Write(buffer.Slice(0, keep));
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Count(buffer.AsSpan(offset, count));
            inner.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Count(buffer);
            inner.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Count(buffer.AsSpan(offset, count));
            return inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Count(buffer.Span);
            return inner.WriteAsync(buffer, cancellationToken);
        }

        public override void Flush() => inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
