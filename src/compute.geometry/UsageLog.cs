using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Serilog;

namespace compute.geometry
{
    // JSON lines appended to a file of this process's own so processes never share a file: one per billable
    // request, a startup record, an overhead record every minute and a final one at shutdown.
    // Fields are only ever added (bumping VERSION), never renamed, so readers can handle every version.
    static class UsageLog
    {
        const int VERSION = 2;
        static readonly TimeSpan OVERHEAD_INTERVAL = TimeSpan.FromMinutes(1);

        static readonly object writeLock = new object();
        static StreamWriter writer;
        static bool failed;

        static readonly object overheadLock = new object();
        static Timer overheadTimer;
        static DateTime periodStartUtc;
        static long periodStartTimestamp;

        public static bool Enabled => !string.IsNullOrEmpty(Config.UsageLogPath);

        public static void Start()
        {
            if (!Enabled)
                return;
            using (var process = Process.GetCurrentProcess())
            {
                var startUtc = process.StartTime.ToUniversalTime();
                Append(new
                {
                    v = VERSION,
                    kind = "startup",
                    time = startUtc,
                    pid = Environment.ProcessId,
                    wallSeconds = Math.Round((DateTime.UtcNow - startUtc).TotalSeconds, 3),
                    cpuSeconds = Math.Round(Environment.CpuUsage.TotalTime.TotalSeconds, 3),
                });
            }
            lock (overheadLock)
            {
                periodStartUtc = DateTime.UtcNow;
                periodStartTimestamp = Stopwatch.GetTimestamp();
                overheadTimer = new Timer(_ => WriteOverhead("overhead"), null, OVERHEAD_INTERVAL, OVERHEAD_INTERVAL);
            }
        }

        public static void Stop() => WriteOverhead("shutdown");

        public static void WriteRequest(DateTime startUtc, string client, string method, string path, int status,
            long ingressBytes, long egressBytes, CpuLedger.Entry cpu, double wallSeconds)
        {
            Append(new
            {
                v = VERSION,
                kind = "request",
                time = startUtc,
                client,
                pid = Environment.ProcessId,
                method,
                path,
                status,
                ingressBytes,
                egressBytes,
                cpuSeconds = Math.Round(cpu.CpuSeconds, 3),
                sharedCpuSeconds = Math.Round(cpu.SharedCpuSeconds, 3),
                waitSeconds = Math.Round(cpu.WaitSeconds, 3),
                wallSeconds = Math.Round(wallSeconds, 3),
            });
        }

        static void WriteOverhead(string kind)
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
                var (idleCpuSeconds, requests) = CpuLedger.TakeOverhead();
                Append(new
                {
                    v = VERSION,
                    kind,
                    time = periodStartUtc,
                    pid = Environment.ProcessId,
                    wallSeconds = Math.Round(Stopwatch.GetElapsedTime(periodStartTimestamp).TotalSeconds, 3),
                    idleCpuSeconds = Math.Round(idleCpuSeconds, 3),
                    requests = requests.ToDictionary(r => r.Key, r => new { count = r.Value.Count, cpuSeconds = Math.Round(r.Value.CpuSeconds, 3) }),
                    processes = ProcessTreeCpu.ActiveProcesses(),
                });
                periodStartUtc = DateTime.UtcNow;
                periodStartTimestamp = Stopwatch.GetTimestamp();
            }
        }

        static void Append(object record)
        {
            string line = JsonSerializer.Serialize(record);
            lock (writeLock)
            {
                if (failed)
                    return;
                try
                {
                    writer ??= Open();
                    writer.WriteLine(line);
                    writer.Flush();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    failed = true;
                    Log.Error(ex, "Usage log: writing to {Path} failed; no further usage records will be written", Config.UsageLogPath);
                }
            }
        }

        static StreamWriter Open()
        {
            Directory.CreateDirectory(Config.UsageLogPath);
            string file = Path.Combine(Config.UsageLogPath, $"usage-{Environment.ProcessId}-{Program.StartTime.ToUniversalTime():yyyyMMddTHHmmss}.jsonl");
            var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
            Log.Information("Usage log: writing usage records to {File}", file);
            return new StreamWriter(stream, new UTF8Encoding(false));
        }
    }
}
