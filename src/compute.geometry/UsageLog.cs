using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Serilog;

namespace compute.geometry
{
    // Usage records as JSON lines: one per billable request, a startup record, an overhead record every minute and
    // a final one at shutdown. They go to compute.meter.agent when it's running, otherwise (or when it can't take
    // them) to a file of this process's own, so processes never share a file. Each record has a sequence number,
    // so the agent stores a record once whichever way it arrives.
    // Fields are only ever added (bumping VERSION), never renamed, so readers can handle every version.
    static class UsageLog
    {
        public const int VERSION = 8;
        static readonly TimeSpan OVERHEAD_INTERVAL = TimeSpan.FromMinutes(1);
        static readonly TimeSpan SHUTDOWN_FLUSH_TIMEOUT = TimeSpan.FromSeconds(2);
        static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromSeconds(30);
        // Records kept while the file can't be written, about 40 MB; past that the oldest are dropped.
        const int MAX_UNWRITTEN = 100_000;
        // A field that doesn't apply, such as a script request's definition, is left out.
        static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        static readonly object appendLock = new object();
        static long nextSeq;
        static StreamWriter writer;
        static readonly Queue<string> unwritten = new Queue<string>();
        static bool failing;
        static DateTime retryUtc;
        static long dropped;

        static readonly object overheadLock = new object();
        static Timer overheadTimer;
        static DateTime periodStartUtc;
        static long periodStartTimestamp;

        // Identifies this process's records, with its pid, in file names and to the agent.
        public static string ProcessStart => Program.StartTime.ToUniversalTime().ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

        public static bool Recording { get; private set; }

        // Also restarts recording after Pause, with a new startup record that covers all CPU used so far.
        public static void Start()
        {
            lock (overheadLock)
            {
                if (Recording)
                    return;
                Recording = true;
                double cpuBefore = CpuLedger.BeginLog();
                using (var process = Process.GetCurrentProcess())
                {
                    var startUtc = process.StartTime.ToUniversalTime();
                    Append(seq => new
                    {
                        v = VERSION,
                        kind = "startup",
                        seq,
                        time = startUtc,
                        pid = Environment.ProcessId,
                        wallSeconds = Math.Round((DateTime.UtcNow - startUtc).TotalSeconds, 3),
                        cpuSeconds = Math.Round(cpuBefore, 3),
                        // For core-hour comparisons when the records are read on another machine.
                        machine = Environment.MachineName,
                        logicalProcessors = Environment.ProcessorCount,
                    });
                }
                periodStartUtc = DateTime.UtcNow;
                periodStartTimestamp = Stopwatch.GetTimestamp();
                overheadTimer = new Timer(_ => WriteOverhead("overhead"), null, OVERHEAD_INTERVAL, OVERHEAD_INTERVAL);
            }
        }

        // Stops recording because metering is off on this machine. Records from before, which the agent hasn't
        // stored yet, go to the fallback file.
        public static void Pause()
        {
            lock (overheadLock)
            {
                if (!Recording)
                    return;
                Recording = false;
                overheadTimer.Dispose();
                overheadTimer = null;
            }
            foreach (string line in AgentLink.Flush(TimeSpan.Zero))
                WriteToFile(line);
            Log.Warning("Usage records: compute.meter.agent says metering is off; not recording usage");
        }

        public static void Stop(string reason)
        {
            WriteOverhead("shutdown", reason);
            foreach (string line in AgentLink.Flush(SHUTDOWN_FLUSH_TIMEOUT))
                WriteToFile(line);
            lock (appendLock)
            {
                if (unwritten.Count > 0 && !TryWrite(Config.UsageLogPath ?? AgentLink.FallbackPath, force: true))
                    Log.Error("Usage log: {Count} usage records couldn't be written before shutdown and are lost", unwritten.Count + dropped);
            }
        }

        public static void WriteRequest(DateTime startUtc, string requestId, string client, string method, string path, int status,
            RequestDefinitions.Use definition, RequestOutcome.Result outcome, long ingressBytes, long egressBytes, CpuLedger.Entry cpu, double wallSeconds)
        {
            Append(seq => new
            {
                v = VERSION,
                kind = "request",
                seq,
                time = startUtc,
                requestId,
                client,
                pid = Environment.ProcessId,
                method,
                path,
                status,
                definition = definition?.Id,
                definitionName = definition?.Name,
                cached = definition?.Cached,
                error = outcome?.Error,
                errors = outcome?.Errors,
                warnings = outcome?.Warnings,
                errorCount = outcome?.ErrorCount,
                warningCount = outcome?.WarningCount,
                ingressBytes,
                egressBytes,
                cpuSeconds = Math.Round(cpu.CpuSeconds, 3),
                sharedCpuSeconds = Math.Round(cpu.SharedCpuSeconds, 3),
                waitSeconds = Math.Round(cpu.WaitSeconds, 3),
                wallSeconds = Math.Round(wallSeconds, 3),
            });
        }

        static void WriteOverhead(string kind, string reason = null)
        {
            lock (overheadLock)
            {
                if (overheadTimer == null)
                    return;
                if (kind == "shutdown")
                {
                    overheadTimer.Dispose();
                    overheadTimer = null;
                }
                var (idleCpuSeconds, requests, totalCpuSeconds) = CpuLedger.TakeOverhead();
                Append(seq => new
                {
                    v = VERSION,
                    kind,
                    seq,
                    time = periodStartUtc,
                    pid = Environment.ProcessId,
                    wallSeconds = Math.Round(Stopwatch.GetElapsedTime(periodStartTimestamp).TotalSeconds, 3),
                    idleCpuSeconds = Math.Round(idleCpuSeconds, 3),
                    requests = requests.ToDictionary(r => r.Key, r => new { count = r.Value.Count, cpuSeconds = Math.Round(r.Value.CpuSeconds, 3) }),
                    processes = ProcessTreeCpu.ActiveProcesses(),
                    totalCpuSeconds = Math.Round(totalCpuSeconds, 3),
                    reason,
                });
                periodStartUtc = DateTime.UtcNow;
                periodStartTimestamp = Stopwatch.GetTimestamp();
            }
        }

        // Numbered and queued under one lock, so records reach the agent in sequence order.
        static void Append(Func<long, object> build)
        {
            lock (appendLock)
            {
                if (!Recording)
                    return;
                long seq = ++nextSeq;
                string line = JsonSerializer.Serialize(build(seq), JSON_OPTIONS);
                if (AgentLink.Attached && AgentLink.Send(seq, line))
                    return;
                WriteToFile(line);
            }
        }

        // A record the file can't take, on a full disk or a folder out of reach, is kept and written with a later one,
        // trying again every RETRY_INTERVAL, so a month of records doesn't stop at the first failure.
        static void WriteToFile(string line)
        {
            string directory = Config.UsageLogPath ?? AgentLink.FallbackPath;
            lock (appendLock)
            {
                if (string.IsNullOrEmpty(directory))
                    return;
                unwritten.Enqueue(line);
                if (unwritten.Count > MAX_UNWRITTEN)
                {
                    unwritten.Dequeue();
                    dropped++;
                }
                TryWrite(directory, force: false);
            }
        }

        // Under appendLock. Records written twice, when a failure comes after some reached the file, are stored once,
        // since the agent keys them by sequence number.
        static bool TryWrite(string directory, bool force)
        {
            if (string.IsNullOrEmpty(directory) || (failing && !force && DateTime.UtcNow < retryUtc))
                return false;
            try
            {
                if (writer == null)
                {
                    writer = Open(directory);
                    // Ends a line an earlier failure may have cut off, so the next record starts a line of its own.
                    if (failing)
                        writer.WriteLine();
                }
                foreach (string line in unwritten)
                    writer.WriteLine(line);
                writer.Flush();
                unwritten.Clear();
                if (failing)
                {
                    if (dropped > 0)
                        Log.Warning("Usage log: writing to {Path} again; {Dropped} usage records were dropped while it couldn't", directory, dropped);
                    else
                        Log.Information("Usage log: writing to {Path} again; no usage records were lost", directory);
                    failing = false;
                    dropped = 0;
                }
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                try
                {
                    writer?.Dispose();
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                }
                writer = null;
                retryUtc = DateTime.UtcNow + RETRY_INTERVAL;
                if (!failing)
                    Log.Error(ex, "Usage log: writing to {Path} failed; keeping usage records and trying again every {Seconds} s", directory, RETRY_INTERVAL.TotalSeconds);
                failing = true;
                return false;
            }
        }

        static StreamWriter Open(string directory)
        {
            Directory.CreateDirectory(directory);
            string file = Path.Combine(directory, $"usage-{Environment.ProcessId}-{ProcessStart}.jsonl");
            var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
            Log.Information("Usage log: writing usage records to {File}", file);
            return new StreamWriter(stream, new UTF8Encoding(false));
        }
    }
}
