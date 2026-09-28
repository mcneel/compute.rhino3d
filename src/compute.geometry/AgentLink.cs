using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Serilog;

namespace compute.geometry
{
    // Sends usage records to compute.meter.agent when one runs on this machine: a named pipe on Windows, a Unix
    // domain socket on Linux. Records wait in memory until the agent says it has stored them and are sent again
    // after a reconnect; the agent ignores ones it already has. Finding the agent turns the usage log on, even in
    // a process that started before it.
    static class AgentLink
    {
        const string PIPE_NAME = "compute.meter.agent";
        const string SOCKET_PATH = "/run/rhino-compute/meter.sock";
        const string AGENT_PROCESS_NAME = "compute.meter.agent";
        const int MAX_WAITING = 10000;
        static readonly TimeSpan LOOK_INTERVAL = TimeSpan.FromSeconds(5);
        static readonly TimeSpan REPLY_TIMEOUT = TimeSpan.FromSeconds(5);

        static readonly object queueLock = new object();
        static readonly LinkedList<(long Seq, string Line)> waiting = new LinkedList<(long, string)>();
        static readonly SemaphoreSlim added = new SemaphoreSlim(0);
        static long sentThrough;
        static int started;
        static string declinedFor;
        static int impostorPid;

        // Set once the agent has been found; from then on records go to it.
        public static bool Attached { get; private set; }
        // Where records go when the agent can't take them: the agent's own usage log directory.
        public static string FallbackPath { get; private set; }

        public static void Start()
        {
            if (Interlocked.Exchange(ref started, 1) == 1)
                return;
            new Thread(Run) { IsBackground = true, Name = "compute.meter.agent link" }.Start();
        }

        // False when too many records are waiting; the caller writes the record to the fallback file instead.
        public static bool Send(long seq, string line)
        {
            lock (queueLock)
            {
                if (waiting.Count >= MAX_WAITING)
                    return false;
                waiting.AddLast((seq, line));
            }
            added.Release();
            return true;
        }

        // Waits for the agent to store what's waiting, and hands back whatever it didn't.
        public static IReadOnlyList<string> Flush(TimeSpan timeout)
        {
            var deadline = Stopwatch.GetTimestamp() + (long)(timeout.TotalSeconds * Stopwatch.Frequency);
            while (Stopwatch.GetTimestamp() < deadline)
            {
                lock (queueLock)
                {
                    if (waiting.Count == 0)
                        return Array.Empty<string>();
                }
                Thread.Sleep(20);
            }
            lock (queueLock)
            {
                var left = waiting.Select(entry => entry.Line).ToList();
                waiting.Clear();
                return left;
            }
        }

        static void Run()
        {
            while (true)
            {
                try
                {
                    if (AgentExists() && Connect() is (Stream stream, StreamReader reader))
                        Serve(stream, reader);
                }
                catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is UnauthorizedAccessException ||
                    ex is JsonException || ex is SocketException || ex is ObjectDisposedException || ex is InvalidOperationException)
                {
                    Log.Debug(ex, "compute.meter.agent connection ended");
                }
                Thread.Sleep(LOOK_INTERVAL);
            }
        }

        // Checks without connecting: opening a pipe's path would take one of the agent's connections.
        static bool AgentExists()
        {
            if (OperatingSystem.IsWindows())
                return WaitNamedPipe($@"\\.\pipe\{PIPE_NAME}", 1) || Marshal.GetLastWin32Error() == ERROR_SEM_TIMEOUT;
            return OperatingSystem.IsLinux() && File.Exists(SOCKET_PATH);
        }

        static (Stream, StreamReader)? Connect()
        {
            Stream stream;
            int serverPid;
            if (OperatingSystem.IsWindows())
            {
                // Asynchronous (overlapped), so reading acknowledgements doesn't block writing records on the same handle.
                var pipe = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect(1000);
                serverPid = GetNamedPipeServerProcessId(pipe.SafePipeHandle, out uint pid) ? (int)pid : 0;
                stream = pipe;
            }
            else
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                socket.Connect(new UnixDomainSocketEndPoint(SOCKET_PATH));
                // SO_PEERCRED fills struct ucred { pid, uid, gid }.
                var credentials = new byte[12];
                socket.GetRawSocketOption(1, 17, credentials);
                serverPid = BitConverter.ToInt32(credentials, 0);
                stream = new NetworkStream(socket, ownsSocket: true);
            }

            // Another program could have taken the name first to collect the records.
            if (!IsAgent(serverPid))
            {
                if (serverPid != impostorPid)
                    Log.Warning("Usage records: {Name} is served by process {Pid}, which isn't compute.meter.agent; not sending to it", PIPE_NAME, serverPid);
                impostorPid = serverPid;
                stream.Dispose();
                return null;
            }

            var reader = new StreamReader(stream, new UTF8Encoding(false));
            WriteLine(stream, JsonSerializer.Serialize(new
            {
                hello = UsageLog.VERSION,
                pid = Environment.ProcessId,
                start = UsageLog.ProcessStart,
            }));
            var replyTask = Task.Run(() => reader.ReadLine());
            if (!replyTask.Wait(REPLY_TIMEOUT) || replyTask.Result == null)
            {
                stream.Dispose();
                return null;
            }
            using var reply = JsonDocument.Parse(replyTask.Result);
            var root = reply.RootElement;
            if (!root.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
            {
                // Metering is off on this machine, or this process mustn't meter (e.g. it was started by another
                // compute.geometry, whose CPU already includes it). Keep looking in case that changes.
                string reason = root.TryGetProperty("error", out var error) ? error.GetString() : "no reason given";
                if (UsageLog.Recording)
                    UsageLog.Pause();
                if (reason != declinedFor)
                    Log.Information("Usage records: compute.meter.agent declined this process: {Reason}", reason);
                declinedFor = reason;
                stream.Dispose();
                return null;
            }
            declinedFor = null;

            long acked = root.TryGetProperty("acked", out var a) && a.ValueKind == JsonValueKind.Number ? a.GetInt64() : 0;
            lock (queueLock)
            {
                while (waiting.First != null && waiting.First.Value.Seq <= acked)
                    waiting.RemoveFirst();
                sentThrough = 0;
            }
            if (FallbackPath == null && root.TryGetProperty("usageLogPath", out var path) && path.ValueKind == JsonValueKind.String)
                FallbackPath = path.GetString();
            Attached = true;
            CpuLedger.Start();
            UsageLog.Start();
            Log.Information("Usage records: sending to compute.meter.agent (process {Pid})", serverPid);
            return (stream, reader);
        }

        // Sends waiting records until the connection breaks; a second thread reads the agent's acknowledgements.
        static void Serve(Stream stream, StreamReader reader)
        {
            var acks = new Thread(() => ReadAcknowledgements(reader)) { IsBackground = true, Name = "compute.meter.agent acknowledgements" };
            acks.Start();
            try
            {
                while (acks.IsAlive)
                {
                    List<(long Seq, string Line)> batch;
                    lock (queueLock)
                        batch = waiting.Where(entry => entry.Seq > sentThrough).ToList();
                    if (batch.Count == 0)
                    {
                        added.Wait(1000);
                        continue;
                    }
                    var text = new StringBuilder();
                    foreach (var entry in batch)
                        text.Append(entry.Line).Append('\n');
                    var bytes = Encoding.UTF8.GetBytes(text.ToString());
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    lock (queueLock)
                        sentThrough = Math.Max(sentThrough, batch[^1].Seq);
                }
            }
            finally
            {
                stream.Dispose();
                Log.Information("Usage records: lost the connection to compute.meter.agent; keeping records until it's back");
            }
        }

        static void ReadAcknowledgements(StreamReader reader)
        {
            try
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    using var ack = JsonDocument.Parse(line);
                    if (!ack.RootElement.TryGetProperty("ack", out var seq) || seq.ValueKind != JsonValueKind.Number)
                        continue;
                    long through = seq.GetInt64();
                    lock (queueLock)
                    {
                        while (waiting.First != null && waiting.First.Value.Seq <= through)
                            waiting.RemoveFirst();
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is ObjectDisposedException)
            {
            }
        }

        static bool IsAgent(int pid)
        {
            try
            {
                using var process = Process.GetProcessById(pid);
                return process.ProcessName == AGENT_PROCESS_NAME;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return false;
            }
        }

        static void WriteLine(Stream stream, string line)
        {
            var bytes = Encoding.UTF8.GetBytes(line + "\n");
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        const int ERROR_SEM_TIMEOUT = 121;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool WaitNamedPipe(string name, uint timeout);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint serverProcessId);
    }
}
