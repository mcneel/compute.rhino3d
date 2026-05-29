using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Serilog;

namespace rhino.compute
{
    static class ComputeChildren
    {
        /// <summary>
        /// Number of child compute.geometry processes to launch
        /// </summary>
        public static int SpawnCount { get; set; } = 1;

        /// <summary>
        /// Upper bound on the number of children that may run at once. Applies to both
        /// the startup <c>--childcount</c> CLI flag and the runtime <c>/launch?children=N</c>
        /// endpoint. Well above any realistic deployment need; typical configs run 1-8.
        /// Raise carefully — beyond this each child uses hundreds of MB and a CPU core.
        /// </summary>
        public const int MaxChildren = 64;

        static DateTime lastCall = DateTime.MinValue;
        public static void UpdateLastCall()
        {
            lastCall = DateTime.Now;
        }

        /// <summary>
        /// Idle time child processes live. If rhino.compute is not called
        /// for this period of time to proxy requests, the child processes will
        /// shut down. The processes will be restarted on a later request
        /// </summary>
        public static TimeSpan ChildIdleSpan { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// This value determines whether a child process should be started
        /// when rhino.compute is first launched. If running in a production
        /// environment, this value should be set to false.
        /// </summary>
        public static bool SpawnOnStartup { get; set; } = false;

        /// <summary>Port that rhino.compute is running on</summary>
        public static int ParentPort { get; set; } = 5000;
        /// <summary>
        /// The system directory for the Rhino executable
        /// </summary>
        public static string RhinoSysDir { get; set; } 
        /// <summary>
        /// Length of time (in seconds) since rhino.compute last made a call
        /// to a child process. The child processes use this information to
        /// figure out if they should exit.
        /// </summary>
        /// <returns>
        /// -1 if a child process has never been called; otherwise
        /// span in seconds since the last call to a child process
        /// </returns>
        public static int IdleSpan()
        {
            if (lastCall == DateTime.MinValue)
                return -1;
            var span = DateTime.Now - lastCall;
            return (int)span.TotalSeconds;
        }
        /// <summary>
        /// Total number of compute.geometry processes being run
        /// </summary>
        public static int ActiveComputeCount
        {
            get
            {
                var processes = Process.GetProcessesByName("compute.geometry");
                return processes.Length;
            }
        }

        /// <summary>
        /// Get base url for a compute server. This function may return a
        /// different string each time it is called as it attempts to provide
        /// basic round robin scheduling when multiple compute servers are
        /// found to be available.
        /// </summary>
        /// <returns></returns>
        public static (string, int) GetComputeServerBaseUrl()
        {
            // Simple round robin scheduler using a queue of compute.geometry processes
            int activePort = 0;

            lock (lockObject)
            {
                if (computeProcesses.Count > 0)
                {
                    Tuple<Process, int> current = computeProcesses.Dequeue();
                    if (!current.Item1.HasExited)
                    {
                        computeProcesses.Enqueue(current);
                        activePort = current.Item2;
                    }
                }

                if (activePort == 0)
                {
                    var aliveProcesses = computeProcesses.Where(tuple => !tuple.Item1.HasExited).ToList();
                    computeProcesses = new Queue<Tuple<Process, int>>(aliveProcesses);
                    LaunchCompute(computeProcesses, true);

                    if (computeProcesses.Count > 0)
                    {
                        Tuple<Process, int> current = computeProcesses.Dequeue();
                        computeProcesses.Enqueue(current);
                        activePort = current.Item2;
                    }
                }
            }

            if (0 == activePort)
                throw new Exception("No compute server found");

            if (computeProcesses.Count < SpawnCount)
            {
                // Bring up other child computes to SpawnCount level
                for(int i=computeProcesses.Count; i<SpawnCount; i++)
                {
                    LaunchCompute(false);
                }
            }

            //Log.Information($"Started child process at http://localhost:{activePort} at {DateTime.Now.ToLocalTime()}");
            return ($"http://localhost:{activePort}", activePort);
        }

        public static void MoveToFrontOfQueue(int port)
        {
            lock (lockObject)
            {
                // TODO: We really should be using a simple list with an index
                // pointing at the next item to use
                if (computeProcesses.Count > 1)
                {
                    for( int i=0; i<computeProcesses.Count; i++)
                    {
                        if (computeProcesses.Peek().Item2 == port)
                            break;
                        var item = computeProcesses.Dequeue();
                        computeProcesses.Enqueue(item);
                    }
                }
            }
        }

        public static void LaunchCompute(bool waitUntilServing)
        {
            lock (lockObject)
            {
                if (computeProcesses.Count >= SpawnCount)
                    return;
                LaunchCompute(computeProcesses, waitUntilServing);
            }
        }

        static void LaunchCompute(Queue<Tuple<Process, int>> processQueue, bool waitUntilServing)
        {
            var pathToThisAssembly = new System.IO.FileInfo(typeof(ComputeChildren).Assembly.Location);
            // compute.geometry is allowed to be either in:
            // - a sibling directory named compute.geometry
            // - a child directory named compute.geometry
            var parentDirectory = pathToThisAssembly.Directory.Parent;
            string pathToCompute = System.IO.Path.Combine(parentDirectory.FullName, "compute.geometry", "compute.geometry.exe");

            if (!System.IO.File.Exists(pathToCompute))
            {
                pathToCompute = System.IO.Path.Combine(pathToThisAssembly.Directory.FullName, "compute.geometry", "compute.geometry.exe");
                if (!System.IO.File.Exists(pathToCompute))
                    return;
            }

            var existingProcesses = processQueue.ToArray();
            var existingPorts = new HashSet<int>();
            foreach (var proc in existingProcesses)
                existingPorts.Add(proc.Item2);

            int port = 0;
            for (int i = 0; i < 256; i++)
            {
                // start at port 6001. Feel free to change this if there is a reason
                // to use a different port
                port = 6001 + i;
                if (i == 255)
                    return;

                if (existingPorts.Contains(port))
                    continue;

                bool isOpen = IsPortOpen("localhost", port, new TimeSpan(0, 0, 0, 0, 100));
                if (isOpen)
                    continue;

                break;
            }

            var startInfo = new ProcessStartInfo(pathToCompute);
            startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = ""; //required for debugging compute.geometry via Visual Studio
            // Redirect the child's stdout/stderr so we can re-emit them through the parent's
            // single Console.Out. Multiple children writing to the OS stdout handle directly
            // would interleave at byte level (visible when SpawnCount >= 2 spawns siblings in
            // parallel, badly visible at 3+); routing through a single in-process TextWriter
            // serializes the writes. The child's own "CG ..." Serilog prefix is preserved.
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            // Sever inheritance of rhino.compute's console handle. Without this, native code
            // inside the child (notably Grasshopper's plugin loader) writes its "Loading X
            // assembly..." progress straight to the inherited CONOUT$ — which bypasses our
            // stdout pipe and ends up shared between all children, causing byte-level
            // interleaving on the parent's console. Forcing CreateNoWindow makes those native
            // writes either fall through to STD_OUTPUT_HANDLE (our pipe) or no-op.
            startInfo.CreateNoWindow = true;
            var rhinoProcess = Process.GetCurrentProcess();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string commandLineArgs = $"-port:{port} -childof:{rhinoProcess.Id}";
            Log.Information($"Starting compute.geometry instance on port {port}");
            if (!string.IsNullOrEmpty(RhinoSysDir))
            {
                commandLineArgs += $" -rhinosysdir:\"{RhinoSysDir}\"";
            }
            if (ParentPort > 0 && ChildIdleSpan.TotalSeconds > 1.0)
            {
                int seconds = (int)ChildIdleSpan.TotalSeconds;
                commandLineArgs += $" -parentport:{ParentPort} -idlespan:{seconds}";
            }
            startInfo.Arguments = commandLineArgs;

            var process = Process.Start(startInfo);
            if (process != null)
            {
                // Lines emitted by the child's Serilog (ANSI theme) already begin with an
                // escape sequence and a "CG {port} [...]" prefix — pass them through verbatim.
                // Anything else (Grasshopper's raw Console.WriteLine output during plugin load,
                // the occasional stderr write) is wrapped via childRawLogger so it picks up
                // the same prefix, colors, and port enrichment as a normal CG line.
                int capturedPort = port;
                void ReEmit(string line)
                {
                    if (line == null) return;
                    if (line.Length > 0 && (line[0] == '\x1B' || line.StartsWith("CG ", StringComparison.Ordinal)))
                        Console.WriteLine(line);
                    else
                        childRawLogger.ForContext("Port", capturedPort).Information("{Line:l}", line);
                }
                process.OutputDataReceived += (s, e) => ReEmit(e.Data);
                process.ErrorDataReceived  += (s, e) => ReEmit(e.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            var start = DateTime.Now;

            if (waitUntilServing)
            {
                while (true)
                {
                    bool isOpen = IsPortOpen("localhost", port, new TimeSpan(0, 0, 1));

                    if (isOpen)
                    {
                        break;
                    }
                        
                    var span = DateTime.Now - start;
                    if (span.TotalSeconds > 60)
                    {
                        process.Kill();
                        string msg = "Unable to start a local compute server";
                        Log.Information(msg);
                        throw new Exception(msg);
                    }
                }
            }
            else
            {
                // no matter what, give compute a little time to start
                System.Threading.Thread.Sleep(100);
            }

            if (process != null)
            {
                processQueue.Enqueue(Tuple.Create(process, port));
            }
        }

        static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
                    return success;
                }
            }
            catch(Exception)
            {
                return false;
            }
        }
        static object lockObject = new object();
        static Queue<Tuple<Process, int>> computeProcesses = new Queue<Tuple<Process, int>>();

        // Wraps raw stdout/stderr lines from child processes (e.g. Grasshopper's plugin-load
        // progress messages written via Console.WriteLine, which bypass the child's Serilog
        // entirely) so they render with the same "CG {Port} [...]" prefix, color theme, and
        // port enrichment as Serilog-emitted CG lines. Uses applyThemeToRedirectedOutput so
        // colors still emit if rhino.compute itself ever runs with redirected stdout.
        static readonly ILogger childRawLogger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "CG {Port} [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate,
                applyThemeToRedirectedOutput: true)
            .CreateLogger();

        /// <summary>
        /// Gracefully shut down all spawned compute.geometry children. POSTs /shutdown to
        /// each child (API key forwarded automatically since children inherit RHINO_COMPUTE_KEY
        /// from the parent's environment, so both ends agree on the key when one is configured).
        /// Waits up to gracefulTimeoutSeconds for each child to exit cleanly, then Kill()'s any
        /// stragglers as a fallback. Called by Program.cs on ApplicationStopping (clean parent
        /// exit). Hard-crash scenarios bypass this entirely — children fall back to the
        /// existing 5-second HasExited poll in their own Shutdown.TimerTask.
        /// </summary>
        public static void ShutdownAllChildren(int gracefulTimeoutSeconds = 3)
        {
            Tuple<Process, int>[] snapshot;
            lock (lockObject)
            {
                snapshot = computeProcesses.ToArray();
                computeProcesses.Clear();
            }
            if (snapshot.Length == 0)
                return;

            Log.Information("Shutting down {Count} compute.geometry child process(es)", snapshot.Length);

            // Short HttpClient timeout: in the Ctrl-C case the child may already be mid-shutdown
            // (Windows broadcasts CTRL_C_EVENT to every process attached to the console), so
            // Kestrel has stopped accepting connections and the POST will hang. We don't need
            // the POST to succeed — the WaitForExit loop below is the actual source of truth.
            // Healthy children respond to /shutdown in milliseconds.
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
            if (!string.IsNullOrEmpty(Config.ApiKey))
                client.DefaultRequestHeaders.Add("RhinoComputeKey", Config.ApiKey);

            foreach (var tuple in snapshot)
            {
                if (tuple.Item1.HasExited) continue;
                try
                {
                    var response = client.PostAsync($"http://localhost:{tuple.Item2}/shutdown", null).GetAwaiter().GetResult();
                    Log.Debug("Shutdown request to compute.geometry on port {Port} returned {Status}", tuple.Item2, (int)response.StatusCode);
                }
                catch (Exception)
                {
                    // Expected when the child is already shutting down via its own signal
                    // handling (typical in Ctrl-C scenarios). The WaitForExit + Kill fallback
                    // below handles both paths uniformly.
                }
            }

            foreach (var tuple in snapshot)
            {
                try
                {
                    if (!tuple.Item1.HasExited && !tuple.Item1.WaitForExit(gracefulTimeoutSeconds * 1000))
                    {
                        Log.Warning("compute.geometry on port {Port} did not exit gracefully within {Timeout}s; killing", tuple.Item2, gracefulTimeoutSeconds);
                        tuple.Item1.Kill(entireProcessTree: true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Error while waiting for compute.geometry on port {Port} to exit: {Message}", tuple.Item2, ex.Message);
                }
            }
        }
    }
}
