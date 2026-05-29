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

        /// <summary>
        /// When true, post-bootstrap child spawns run one at a time on a single background
        /// task — each LaunchCompute() call blocks until that child's port is open before the
        /// next is started. When false (default), the (SpawnCount-1) post-bootstrap spawns are
        /// dispatched as separate Task.Run calls and run in parallel.
        /// Sequential is the escape hatch for memory-constrained VMs where parallel
        /// Rhino+Grasshopper loads can exhaust RAM.
        /// </summary>
        public static bool LoadChildrenSequentially { get; set; } = false;

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
                    // Prune any dead processes before the bootstrap launch below
                    var aliveProcesses = computeProcesses.Where(tuple => !tuple.Item1.HasExited).ToList();
                    computeProcesses = new Queue<Tuple<Process, int>>(aliveProcesses);
                }
            }

            if (activePort == 0)
            {
                // Bootstrap: no running children — launch one synchronously (outside the lock
                // so we don't hold lockObject for the full startup wait) then pick it up.
                LaunchCompute();

                lock (lockObject)
                {
                    // If a spawn is in-flight but not yet ready, wait for it to complete
                    // rather than failing immediately. PulseAll is called in LaunchCompute's
                    // finally block, so we will be woken when the spawn succeeds or fails.
                    while (computeProcesses.Count == 0 && pendingSpawnPorts.Count > 0)
                        Monitor.Wait(lockObject, millisecondsTimeout: 1000);

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

            // Compute how many background spawns are needed under the lock so we don't
            // schedule redundant tasks based on a stale unsynchronised count.
            int spawnTasksToQueue;
            lock (lockObject)
            {
                spawnTasksToQueue = Math.Max(0, SpawnCount - (computeProcesses.Count + pendingSpawnPorts.Count));
            }
            if (spawnTasksToQueue > 0)
            {
                if (LoadChildrenSequentially)
                {
                    // Single background task that drains the queue serially. LaunchCompute()
                    // blocks until the spawned child's port opens, so each iteration waits for
                    // the previous to finish — caps peak Rhino+Grasshopper memory at one extra
                    // child at a time.
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        for (int i = 0; i < spawnTasksToQueue; i++)
                            LaunchCompute();
                    });
                }
                else
                {
                    for (int i = 0; i < spawnTasksToQueue; i++)
                        System.Threading.Tasks.Task.Run(() => LaunchCompute());
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

        public static void LaunchCompute()
        {
            // Resolve path before acquiring the lock to keep lock duration short.
            string pathToCompute = FindComputeExecutablePath();
            if (pathToCompute == null) return;

            // Under a brief lock: check whether we need another child and reserve a port.
            // pendingSpawnPorts tracks ports currently being started in background tasks so
            // we do not double-reserve them or exceed SpawnCount.
            // Snapshot listening ports before the lock to avoid an OS syscall inside it.
            var listeningPorts = GetListeningPorts();
            int port;
            lock (lockObject)
            {
                if (computeProcesses.Count + pendingSpawnPorts.Count >= SpawnCount)
                    return;

                var usedPorts = new HashSet<int>(computeProcesses.Select(t => t.Item2));
                usedPorts.UnionWith(pendingSpawnPorts);
                port = FindFreePort(usedPorts, listeningPorts);
                if (port == 0) return;
                pendingSpawnPorts.Add(port);
            }

            StartChildOnReservedPort(pathToCompute, port);
        }

        // Performs the actual spawn for a port that has ALREADY been added to pendingSpawnPorts
        // under the lock. Spawns outside the lock; in finally, re-acquires the lock to remove
        // the reservation and enqueue on success. Always pulses lockObject so threads waiting
        // in GetComputeServerBaseUrl (via Monitor.Wait) are woken whether we succeed or fail.
        // Shared between LaunchCompute() (auto-fill path) and LaunchChild() (manual path).
        static void StartChildOnReservedPort(string pathToCompute, int port)
        {
            Process process = null;
            bool started = false;
            try
            {
                started = TryStartChild(pathToCompute, port, out process);
                if (!started)
                    Log.Warning("compute.geometry on port {Port} failed to start within 60 seconds", port);
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception ||
                                       ex is InvalidOperationException)
            {
                Log.Error(ex, "Exception while starting compute.geometry on port {Port}", port);
            }
            finally
            {
                lock (lockObject)
                {
                    pendingSpawnPorts.Remove(port);
                    if (started && process != null && !process.HasExited)
                        computeProcesses.Enqueue(Tuple.Create(process, port));
                    // Wake any threads waiting in GetComputeServerBaseUrl for this spawn to finish.
                    Monitor.PulseAll(lockObject);
                }
            }
        }

        // Looks for compute.geometry in a sibling directory named compute.geometry relative to
        // this assembly's parent directory. Returns null if the executable cannot be found.
        static string FindComputeExecutablePath()
        {
            var pathToThisAssembly = new System.IO.FileInfo(typeof(ComputeChildren).Assembly.Location);
            var parentDirectory = pathToThisAssembly.Directory?.Parent;
            if (parentDirectory == null)
            {
                Log.Warning("Could not determine parent directory of assembly {Assembly}; cannot locate compute.geometry", pathToThisAssembly.FullName);
                return null;
            }

            string computeDirectoryPath = System.IO.Path.Join(parentDirectory.FullName, "compute.geometry");
            string path = System.IO.Path.Join(computeDirectoryPath, "compute.geometry");
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                path += ".exe";

            if (!System.IO.File.Exists(path))
            {
                Log.Warning("compute.geometry executable not found at {Path}", path);
                return null;
            }

            return path;
        }

        // Shared helper: creates the start info, starts the process on the given port, and waits
        // for the port to open. Returns true on success, false if the port never opened (timeout).
        static bool TryStartChild(string pathToCompute, int port, out Process process)
        {
            var startInfo = CreateComputeStartInfo(pathToCompute, port);
            process = Process.Start(startInfo);
            if (process != null)
            {
                // Lines emitted by the child's Serilog (ANSI theme) already begin with an
                // escape sequence and a "CG {port} [...]" prefix — pass them through verbatim.
                // Anything else (Grasshopper's raw Console.WriteLine output during plugin load,
                // the occasional stderr write) is wrapped via childRawLogger so it picks up
                // the same prefix, colors, and port enrichment as a normal CG line.
                void ReEmit(string line)
                {
                    if (line == null) return;
                    if (line.Length > 0 && (line[0] == '\x1B' || line.StartsWith("CG ", StringComparison.Ordinal)))
                        Console.WriteLine(line);
                    else
                        childRawLogger.ForContext("Port", port).Information("{Line:l}", line);
                }
                process.OutputDataReceived += (s, e) => ReEmit(e.Data);
                process.ErrorDataReceived  += (s, e) => ReEmit(e.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            return WaitForChildProcess(process, port);
        }

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

        // Returns the first port >= 6001 that is not in usedPorts and is not already listening.
        // Returns 0 if no free port is found.
        // Callers that already hold a lock should pass a pre-fetched listeningPorts snapshot
        // to avoid an OS syscall inside the lock.
        static int FindFreePort(HashSet<int> usedPorts, HashSet<int> listeningPorts = null)
        {
            listeningPorts ??= GetListeningPorts();

            for (int i = 0; i < 256; i++)
            {
                int port = 6001 + i;
                if (usedPorts.Contains(port)) continue;
                if (listeningPorts.Contains(port)) continue;
                return port;
            }
            return 0;
        }

        static ProcessStartInfo CreateComputeStartInfo(string pathToCompute, int port)
        {
            var startInfo = new ProcessStartInfo(pathToCompute);
            startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "";
            // Redirect the child's stdout/stderr so we can re-emit them through the parent's
            // single Console.Out. Multiple children writing to the OS stdout handle directly
            // would interleave at byte level (visible when SpawnCount >= 3 spawns siblings in
            // parallel); routing through a single in-process TextWriter serializes the writes.
            // The child's own "CG ..." Serilog prefix is preserved.
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
            string args = $"-port:{port} -childof:{rhinoProcess.Id}";
            Log.Information("Starting compute.geometry instance on port {Port}", port);
            if (!string.IsNullOrEmpty(RhinoSysDir))
                args += $" -rhinosysdir:\"{RhinoSysDir}\"";
            if (ParentPort > 0 && ChildIdleSpan.TotalSeconds > 1.0)
                args += $" -parentport:{ParentPort} -idlespan:{(int)ChildIdleSpan.TotalSeconds}";
            startInfo.Arguments = args;
            return startInfo;
        }

        // Polls until the child process port is confirmed open, or kills the process after timeout.
        // Returns true if the port opened within the timeout, false otherwise.
        static bool WaitForChildProcess(Process process, int port, int timeoutSeconds = 60)
        {
            var start = DateTime.Now;
            while (true)
            {
                if (process == null || process.HasExited)
                    return false;

                if (IsPortOpen(port))
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeoutSeconds)
                {
                    try { process.Kill(); } catch (Exception ex) { Log.Debug(ex, "Exception killing timed-out compute.geometry process on port {Port}", port); }
                    process.Dispose();
                    return false;
                }

                Thread.Sleep(1000);
            }
        }

        // Returns a snapshot of all ports with active TCP listeners.
        // Returns an empty set if the OS query fails, so callers degrade gracefully.
        static HashSet<int> GetListeningPorts()
        {
            try
            {
                var listeners = System.Net.NetworkInformation.IPGlobalProperties
                    .GetIPGlobalProperties()
                    .GetActiveTcpListeners();
                return new HashSet<int>(listeners.Select(ep => ep.Port));
            }
            catch (System.Net.NetworkInformation.NetworkInformationException ex)
            {
                Log.Warning(ex, "Failed to enumerate TCP listeners; treating all ports as available");
                return new HashSet<int>();
            }
        }

        // Returns true if any TCP listener is currently bound to the given port.
        static bool IsPortOpen(int port) => GetListeningPorts().Contains(port);

        /// <summary>
        /// Gracefully shut down compute.geometry children. POSTs /shutdown to each child
        /// (API key forwarded automatically since children inherit RHINO_COMPUTE_KEY from the
        /// parent's environment, so both ends agree on the key when one is configured). Waits
        /// up to <paramref name="gracefulTimeoutSeconds"/> for each child to exit cleanly,
        /// then Kill()'s any stragglers as a fallback.
        ///
        /// <para>Called by Program.cs on ApplicationStopping (clean parent exit, no respawn),
        /// and by the /shutdown-children and /recycle-children endpoints (with respawn=true for
        /// the latter). Hard-crash scenarios bypass this entirely — children fall back to the
        /// existing 5-second HasExited poll in their own Shutdown.TimerTask.</para>
        /// </summary>
        /// <param name="portFilter">When non-null, only the child on this port is shut down
        /// (others are left running). When null, all children are shut down.</param>
        /// <param name="respawn">When true, spawn one fresh replacement per shut-down child,
        /// sequentially (one at a time) so the queue is never empty mid-recycle if other
        /// children are handling traffic. Respects SpawnCount.</param>
        /// <returns>Ports that were shut down, and ports of any newly-spawned replacements.</returns>
        public static (int[] shutdown, int[] spawned) ShutdownChildren(
            int? portFilter = null,
            bool respawn = false,
            int gracefulTimeoutSeconds = 3)
        {
            Tuple<Process, int>[] toShutdown;
            lock (lockObject)
            {
                if (portFilter.HasValue)
                {
                    var match = new List<Tuple<Process, int>>();
                    var keep = new Queue<Tuple<Process, int>>();
                    foreach (var t in computeProcesses)
                    {
                        if (t.Item2 == portFilter.Value)
                            match.Add(t);
                        else
                            keep.Enqueue(t);
                    }
                    computeProcesses = keep;
                    toShutdown = match.ToArray();
                }
                else
                {
                    toShutdown = computeProcesses.ToArray();
                    computeProcesses.Clear();
                }
            }
            if (toShutdown.Length == 0)
                return (Array.Empty<int>(), Array.Empty<int>());

            Log.Information("Shutting down {Count} compute.geometry child process(es)", toShutdown.Length);

            // Short HttpClient timeout: in the Ctrl-C case the child may already be mid-shutdown
            // (Windows broadcasts CTRL_C_EVENT to every process attached to the console), so
            // Kestrel has stopped accepting connections and the POST will hang. We don't need
            // the POST to succeed — the WaitForExit loop below is the actual source of truth.
            // Healthy children respond to /shutdown in milliseconds.
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
            if (!string.IsNullOrEmpty(Config.ApiKey))
                client.DefaultRequestHeaders.Add("RhinoComputeKey", Config.ApiKey);

            foreach (var tuple in toShutdown)
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

            foreach (var tuple in toShutdown)
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

            var shutdownPorts = toShutdown.Select(t => t.Item2).ToArray();
            int[] spawnedPorts = Array.Empty<int>();
            if (respawn)
            {
                var spawned = new List<int>();
                foreach (var _ in shutdownPorts)
                {
                    int countBefore;
                    lock (lockObject)
                        countBefore = computeProcesses.Count;
                    try
                    {
                        LaunchCompute();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Error during recycle respawn: {Message}", ex.Message);
                        break;
                    }
                    lock (lockObject)
                    {
                        if (computeProcesses.Count > countBefore)
                            spawned.Add(computeProcesses.Last().Item2);
                    }
                }
                spawnedPorts = spawned.ToArray();
            }

            return (shutdownPorts, spawnedPorts);
        }

        /// <summary>
        /// Fill the compute.geometry child pool up to <see cref="SpawnCount"/>. If already at
        /// or above SpawnCount, no-op (returns empty array). Used by the POST /launch-children
        /// endpoint. Honors <see cref="LoadChildrenSequentially"/> — same flag, same semantics
        /// as the auto-spawn path in <see cref="GetComputeServerBaseUrl"/>: parallel by default
        /// for fast warmup, sequential when the flag is set. Always blocks until all spawns
        /// complete so the response can report the spawned ports accurately.
        /// </summary>
        /// <returns>Ports newly spawned by this call.</returns>
        public static int[] LaunchChildren()
        {
            int targetSpawns;
            int[] existingPortsSnapshot;
            lock (lockObject)
            {
                int currentCount = computeProcesses.Count + pendingSpawnPorts.Count;
                targetSpawns = Math.Max(0, SpawnCount - currentCount);
                existingPortsSnapshot = computeProcesses.Select(t => t.Item2).ToArray();
            }
            if (targetSpawns == 0)
                return Array.Empty<int>();

            if (LoadChildrenSequentially)
            {
                // One spawn at a time — caps peak Rhino+Grasshopper memory at one extra child
                // mid-load. Each LaunchCompute() call blocks until the spawned child's port opens.
                for (int i = 0; i < targetSpawns; i++)
                    LaunchCompute();
            }
            else
            {
                // Parallel spawning for fast warmup. Each task does its own port reservation +
                // start + enqueue under the pendingSpawnPorts protocol, so concurrent spawns
                // can't collide on a port.
                var tasks = new System.Threading.Tasks.Task[targetSpawns];
                for (int i = 0; i < targetSpawns; i++)
                    tasks[i] = System.Threading.Tasks.Task.Run(() => LaunchCompute());
                System.Threading.Tasks.Task.WaitAll(tasks);
            }

            // Diff the queue against the pre-spawn snapshot to find newly-added ports.
            // (Order is non-deterministic in parallel mode; that's fine.)
            lock (lockObject)
            {
                var existingSet = new HashSet<int>(existingPortsSnapshot);
                return computeProcesses
                    .Select(t => t.Item2)
                    .Where(p => !existingSet.Contains(p))
                    .ToArray();
            }
        }

        /// <summary>
        /// Spawn exactly one compute.geometry child. Bypasses the SpawnCount auto-fill cap so
        /// callers can push above the configured baseline, but enforces the absolute MaxChildren
        /// ceiling. Used by the POST /launch-child endpoint.
        /// </summary>
        /// <param name="requestedPort">Optional port to bind. When null, uses the next available
        /// port in the 6001-6256 range. When specified, validates that the port is in 6001-65535,
        /// not held by an existing child, not already being spawned, and not already bound externally.</param>
        /// <returns>The port the new child was spawned on.</returns>
        /// <exception cref="ArgumentOutOfRangeException">When requestedPort is outside 6001-65535.</exception>
        /// <exception cref="InvalidOperationException">When the port is already in use, the pool
        /// is at MaxChildren, no port is available, or compute.geometry.exe is missing.</exception>
        public static int LaunchChild(int? requestedPort)
        {
            string pathToCompute = FindComputeExecutablePath();
            if (pathToCompute == null)
                throw new InvalidOperationException("compute.geometry executable not found.");

            var listeningPorts = GetListeningPorts();
            int port;
            lock (lockObject)
            {
                if (computeProcesses.Count + pendingSpawnPorts.Count >= MaxChildren)
                    throw new InvalidOperationException($"Maximum child count reached ({MaxChildren}).");

                if (requestedPort.HasValue)
                {
                    if (requestedPort.Value < 6001 || requestedPort.Value > 65535)
                        throw new ArgumentOutOfRangeException(nameof(requestedPort),
                            "Port must be in range 6001-65535.");
                    if (computeProcesses.Any(t => t.Item2 == requestedPort.Value))
                        throw new InvalidOperationException(
                            $"Port {requestedPort.Value} is already in use by an existing child.");
                    if (pendingSpawnPorts.Contains(requestedPort.Value))
                        throw new InvalidOperationException(
                            $"Port {requestedPort.Value} is already being spawned.");
                    if (listeningPorts.Contains(requestedPort.Value))
                        throw new InvalidOperationException(
                            $"Port {requestedPort.Value} is already in use.");
                    port = requestedPort.Value;
                }
                else
                {
                    var usedPorts = new HashSet<int>(computeProcesses.Select(t => t.Item2));
                    usedPorts.UnionWith(pendingSpawnPorts);
                    port = FindFreePort(usedPorts, listeningPorts);
                    if (port == 0)
                        throw new InvalidOperationException(
                            "No available port found in the 6001-6256 range.");
                }
                pendingSpawnPorts.Add(port);
            }

            StartChildOnReservedPort(pathToCompute, port);

            // Verify the spawn actually landed in the queue (startup might have timed out
            // or failed inside StartChildOnReservedPort, in which case it's gone from
            // pendingSpawnPorts but not in computeProcesses).
            lock (lockObject)
            {
                if (!computeProcesses.Any(t => t.Item2 == port))
                    throw new InvalidOperationException(
                        $"Failed to spawn child on port {port} (startup timed out or process exited).");
            }
            return port;
        }

        /// <summary>Returns the current count of tracked compute.geometry children.</summary>
        public static int CurrentChildCount
        {
            get
            {
                lock (lockObject)
                    return computeProcesses.Count;
            }
        }

        static object lockObject = new object();
        static Queue<Tuple<Process, int>> computeProcesses = new Queue<Tuple<Process, int>>();
        // Ports for which a child process has been started but has not yet been confirmed
        // ready and added to computeProcesses. Protected by lockObject.
        static readonly HashSet<int> pendingSpawnPorts = new HashSet<int>();
    }
}
