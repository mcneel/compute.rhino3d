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
    // a final one at shutdown, each with the billing group and user (sub) from RHINO_TOKEN. They go to this process's own
    // file for the day (UTC), so processes never share a file; a later day's file starts with a "continued" record. Each
    // record has a sequence number, so a reader stores a record once however often it reads the file.
    // When the drive runs low, the oldest files are deleted to keep the newest records, and if that isn't enough,
    // records are skipped until there's space again; a "removed" or "gap" record says what was lost.
    // Fields are only ever added (bumping VERSION), never renamed, so readers can handle every version.
    static class UsageLog
    {
        public const int VERSION = 1;
        static readonly TimeSpan OVERHEAD_INTERVAL = TimeSpan.FromMinutes(1);
        static readonly TimeSpan RETRY_INTERVAL = TimeSpan.FromSeconds(30);
        static readonly TimeSpan DISK_CHECK_INTERVAL = TimeSpan.FromMinutes(1);
        // Every process writes at least once a minute, so a file written more recently may still be in use.
        static readonly TimeSpan IN_USE = TimeSpan.FromMinutes(2);
        // Deleting frees this much beyond the minimum, so it doesn't run again a minute later.
        const long FREE_MARGIN = 256L * 1024 * 1024;
        // Records kept while the file can't be written, about 40 MB; past that the oldest are dropped.
        const int MAX_UNWRITTEN = 100_000;
        const string FILE_PATTERN = "usage-*.jsonl";
        // A field that doesn't apply, such as a script request's definition, is left out.
        static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        static readonly object appendLock = new object();
        static long nextSeq;
        static StreamWriter writer;
        static string writerPath;
        static DateTime fileDay;
        static readonly Queue<string> unwritten = new Queue<string>();
        static bool failing;
        static DateTime retryUtc;
        static long dropped;
        static DateTime diskCheckedUtc;
        static bool diskLow;
        static long skipped;
        static DateTime skippedSinceUtc;

        static readonly object overheadLock = new object();
        static Timer overheadTimer;
        static DateTime periodStartUtc;
        static long periodStartTimestamp;
        static DateTime processStartUtc;

        // Identifies this process's records, with its pid, in file names.
        public static string ProcessStart => Program.StartTime.ToUniversalTime().ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

        public static bool Recording { get; private set; }

        public static void Start()
        {
            lock (overheadLock)
            {
                if (Recording)
                    return;
                double cpuBefore = CpuLedger.BeginLog();
                using (var process = Process.GetCurrentProcess())
                    processStartUtc = process.StartTime.ToUniversalTime();
                lock (appendLock)
                    fileDay = DateTime.UtcNow.Date;
                Recording = true;
                Append(seq => new
                {
                    v = VERSION,
                    kind = "startup",
                    seq,
                    time = processStartUtc,
                    groupId = Config.GroupId,
                    sub = Config.TokenSubject,
                    pid = Environment.ProcessId,
                    wallSeconds = Math.Round((DateTime.UtcNow - processStartUtc).TotalSeconds, 3),
                    cpuSeconds = Math.Round(cpuBefore, 3),
                    // For core-hour comparisons when the records are read on another machine.
                    machine = Environment.MachineName,
                    logicalProcessors = Environment.ProcessorCount,
                });
                periodStartUtc = DateTime.UtcNow;
                periodStartTimestamp = Stopwatch.GetTimestamp();
                overheadTimer = new Timer(_ => WriteOverhead("overhead"), null, OVERHEAD_INTERVAL, OVERHEAD_INTERVAL);
            }
        }

        public static void Stop(string reason)
        {
            WriteOverhead("shutdown", reason);
            lock (appendLock)
            {
                if (unwritten.Count > 0 && !TryWrite(force: true))
                    Log.Error("Usage log: {Count} usage records couldn't be written before shutdown and are lost", unwritten.Count + dropped);
            }
        }

        // Body sizes, unpacked and as sent; the encodings say how each was sent ("gzip", or "none", when both sizes are the
        // same).
        public static void WriteRequest(DateTime startUtc, string requestId, JsonElement? metadata, string method, string path, int status,
            RequestDefinitions.Use definition, RequestMcp.Use mcp, RequestOutcome.Result outcome, long uncompressedRequestBytes, long compressedRequestBytes,
            string requestEncoding, long uncompressedResponseBytes, long compressedResponseBytes, string responseEncoding, CpuLedger.Entry cpu,
            double wallSeconds)
        {
            Append(seq => new
            {
                v = VERSION,
                kind = "request",
                seq,
                time = startUtc,
                groupId = Config.GroupId,
                sub = Config.TokenSubject,
                requestId,
                pid = Environment.ProcessId,
                method,
                path,
                status,
                metadata,
                definition = definition?.Id,
                definitionName = definition?.Name,
                cached = definition?.Cached,
                mcp = mcp == null ? null : new { session = mcp.Session, method = mcp.Method, tool = mcp.Tool, error = mcp.Error },
                error = outcome?.Error,
                errors = outcome?.Errors,
                warnings = outcome?.Warnings,
                errorCount = outcome?.ErrorCount,
                warningCount = outcome?.WarningCount,
                uncompressedRequestBytes,
                compressedRequestBytes,
                requestEncoding,
                uncompressedResponseBytes,
                compressedResponseBytes,
                responseEncoding,
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
                    groupId = Config.GroupId,
                    sub = Config.TokenSubject,
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

        // Numbered and queued under one lock, so records reach the file in sequence order.
        static void Append(Func<long, object> build)
        {
            lock (appendLock)
            {
                if (!Recording || !PrepareFile())
                    return;
                long seq = ++nextSeq;
                WriteToFile(JsonSerializer.Serialize(build(seq), JSON_OPTIONS));
            }
        }

        // Under appendLock, before a record for the file: starts a new file when the day changes, and keeps the drive
        // from filling. False when the record has to be skipped for lack of space.
        static bool PrepareFile()
        {
            var now = DateTime.UtcNow;
            if (now.Date != fileDay)
            {
                CloseWriter();
                fileDay = now.Date;
                Queue(seq => new
                {
                    v = VERSION,
                    kind = "continued",
                    seq,
                    time = now,
                    groupId = Config.GroupId,
                    sub = Config.TokenSubject,
                    pid = Environment.ProcessId,
                    processStart = processStartUtc,
                    machine = Environment.MachineName,
                    logicalProcessors = Environment.ProcessorCount,
                });
            }
            if (Config.UsageLogMinFreeBytes > 0 && now - diskCheckedUtc >= DISK_CHECK_INTERVAL)
            {
                diskCheckedUtc = now;
                CheckDisk(now);
            }
            if (diskLow)
            {
                skipped++;
                return false;
            }
            return true;
        }

        // Under appendLock: a record for the file, ahead of the one being appended.
        static void Queue(Func<long, object> build) => unwritten.Enqueue(JsonSerializer.Serialize(build(++nextSeq), JSON_OPTIONS));

        // Under appendLock.
        static void CheckDisk(DateTime now)
        {
            string directory = Config.UsageLogPath;
            long minimum = Config.UsageLogMinFreeBytes;
            long free = FreeSpace(directory);
            if (free < 0)
                return;
            if (free < minimum)
            {
                var removed = DeleteOldest(directory, minimum + FREE_MARGIN - free, now);
                if (removed.Files > 0)
                {
                    Log.Warning("Usage log: {Free} MB free, under the {Minimum} MB minimum; deleted the {Files} oldest usage files ({Deleted} MB, last written {From:yyyy-MM-dd} to {To:yyyy-MM-dd})",
                        free / (1024 * 1024), minimum / (1024 * 1024), removed.Files, removed.Bytes / (1024 * 1024), removed.From, removed.To);
                    Queue(seq => new
                    {
                        v = VERSION,
                        kind = "removed",
                        seq,
                        time = now,
                        groupId = Config.GroupId,
                        sub = Config.TokenSubject,
                        pid = Environment.ProcessId,
                        files = removed.Files,
                        bytes = removed.Bytes,
                        from = removed.From,
                        to = removed.To,
                    });
                    free = FreeSpace(directory);
                }
            }
            if (free >= 0 && free < minimum)
            {
                if (!diskLow)
                {
                    diskLow = true;
                    skippedSinceUtc = now;
                    Log.Warning("Usage log: {Free} MB free and no older usage files left to delete; skipping usage records until there are {Minimum} MB free",
                        free / (1024 * 1024), minimum / (1024 * 1024));
                }
            }
            else if (diskLow)
            {
                diskLow = false;
                Log.Information("Usage log: writing usage records again; {Skipped} were skipped while the drive was low on space", skipped);
                Queue(seq => new
                {
                    v = VERSION,
                    kind = "gap",
                    seq,
                    time = skippedSinceUtc,
                    groupId = Config.GroupId,
                    sub = Config.TokenSubject,
                    pid = Environment.ProcessId,
                    wallSeconds = Math.Round((now - skippedSinceUtc).TotalSeconds, 3),
                    skipped,
                });
                skipped = 0;
            }
        }

        // Oldest first, until about wanted bytes are freed. Files still in use, by this process or another, are kept.
        static (int Files, long Bytes, DateTime From, DateTime To) DeleteOldest(string directory, long wanted, DateTime now)
        {
            FileInfo[] candidates;
            try
            {
                candidates = new DirectoryInfo(directory).GetFiles(FILE_PATTERN)
                    .Where(f => now - f.LastWriteTimeUtc > IN_USE && !string.Equals(f.FullName, writerPath, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f.LastWriteTimeUtc)
                    .ToArray();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return (0, 0, default, default);
            }
            int files = 0;
            long bytes = 0;
            DateTime from = default, to = default;
            foreach (var file in candidates)
            {
                if (bytes >= wanted)
                    break;
                try
                {
                    long length = file.Length;
                    var written = file.LastWriteTimeUtc;
                    file.Delete();
                    if (files++ == 0)
                        from = written;
                    to = written;
                    bytes += length;
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                }
            }
            return (files, bytes, from, to);
        }

        // Free space on the drive holding directory, or -1 if it can't be found.
        static long FreeSpace(string directory)
        {
            try
            {
                string path = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                DriveInfo drive = null;
                int matched = -1;
                foreach (var candidate in DriveInfo.GetDrives())
                {
                    string root = candidate.RootDirectory.FullName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    if (root.Length > matched && path.StartsWith(root, comparison))
                    {
                        drive = candidate;
                        matched = root.Length;
                    }
                }
                return drive?.AvailableFreeSpace ?? -1;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return -1;
            }
        }

        // A record the file can't take, on a full disk or a folder out of reach, is kept and written with a later one,
        // trying again every RETRY_INTERVAL, so a month of records doesn't stop at the first failure.
        static void WriteToFile(string line)
        {
            lock (appendLock)
            {
                unwritten.Enqueue(line);
                if (unwritten.Count > MAX_UNWRITTEN)
                {
                    unwritten.Dequeue();
                    dropped++;
                }
                TryWrite(force: false);
            }
        }

        // Under appendLock. Records written twice, when a failure comes after some reached the file, are stored once,
        // since readers key them by sequence number.
        static bool TryWrite(bool force)
        {
            string directory = Config.UsageLogPath;
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
                CloseWriter();
                retryUtc = DateTime.UtcNow + RETRY_INTERVAL;
                if (!failing)
                    Log.Error(ex, "Usage log: writing to {Path} failed; keeping usage records and trying again every {Seconds} s", directory, RETRY_INTERVAL.TotalSeconds);
                failing = true;
                return false;
            }
        }

        static void CloseWriter()
        {
            try
            {
                writer?.Dispose();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
            }
            writer = null;
        }

        // The day first, so a folder of files sorts by date.
        static StreamWriter Open(string directory)
        {
            Directory.CreateDirectory(directory);
            string file = Path.Combine(directory, $"usage-{fileDay.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-{Environment.ProcessId}-{ProcessStart}.jsonl");
            var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
            writerPath = Path.GetFullPath(file);
            Log.Information("Usage log: writing usage records to {File}", file);
            return new StreamWriter(stream, new UTF8Encoding(false));
        }
    }
}
