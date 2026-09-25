using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Serilog;

namespace compute.geometry
{
    // One JSON line per request, appended to a file of this process's own so processes never share a file.
    // Fields are only ever added (bumping VERSION), never renamed, so readers can handle every version.
    static class UsageLog
    {
        const int VERSION = 1;

        static readonly object writeLock = new object();
        static StreamWriter writer;
        static bool failed;

        public static bool Enabled => !string.IsNullOrEmpty(Config.UsageLogPath);

        public static void Write(DateTime startUtc, string client, string method, string path, int status,
            long ingressBytes, long egressBytes, double cpuSeconds, double wallSeconds)
        {
            string line = JsonSerializer.Serialize(new
            {
                v = VERSION,
                time = startUtc,
                client,
                pid = Environment.ProcessId,
                method,
                path,
                status,
                ingressBytes,
                egressBytes,
                cpuSeconds = Math.Round(cpuSeconds, 3),
                wallSeconds = Math.Round(wallSeconds, 3),
            });

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
