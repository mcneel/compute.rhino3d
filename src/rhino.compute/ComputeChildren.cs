using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        static DateTime _lastCall = DateTime.MinValue;
        public static void UpdateLastCall()
        {
            _lastCall = DateTime.Now;
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
            if (_lastCall == DateTime.MinValue)
                return -1;
            var span = DateTime.Now - _lastCall;
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

            lock (_lockObject)
            {
                if (_computeProcesses.Count > 0)
                {
                    Tuple<Process, int> current = _computeProcesses.Dequeue();
                    if (!current.Item1.HasExited)
                    {
                        _computeProcesses.Enqueue(current);
                        activePort = current.Item2;
                    }
                }

                if (activePort == 0)
                {
                    // Prune any dead processes before the bootstrap launch below
                    var aliveProcesses = _computeProcesses.Where(tuple => !tuple.Item1.HasExited).ToList();
                    _computeProcesses = new Queue<Tuple<Process, int>>(aliveProcesses);
                }
            }

            if (activePort == 0)
            {
                // Bootstrap: no running children — launch one synchronously (outside the lock
                // so we don't hold _lockObject for the full startup wait) then pick it up.
                LaunchCompute();

                lock (_lockObject)
                {
                    if (_computeProcesses.Count > 0)
                    {
                        Tuple<Process, int> current = _computeProcesses.Dequeue();
                        _computeProcesses.Enqueue(current);
                        activePort = current.Item2;
                    }
                }
            }

            if (0 == activePort)
                throw new Exception("No compute server found");

            // Bring up remaining children to SpawnCount in the background. Each task waits until
            // the child is confirmed ready (IsPortOpen) before adding it to the round-robin queue,
            // so no request is ever proxied to a port that isn't listening yet.
            // LaunchCompute(bool) guards with _lockObject internally, so extra Task.Run calls are safe.
            for (int i = _computeProcesses.Count + _pendingSpawnPorts.Count; i < SpawnCount; i++)
                System.Threading.Tasks.Task.Run(() => LaunchCompute());

            //Log.Information($"Started child process at http://localhost:{activePort} at {DateTime.Now.ToLocalTime()}");
            return ($"http://localhost:{activePort}", activePort);
        }

        public static void MoveToFrontOfQueue(int port)
        {
            lock (_lockObject)
            {
                // TODO: We really should be using a simple list with an index
                // pointing at the next item to use
                if (_computeProcesses.Count > 1)
                {
                    for( int i=0; i<_computeProcesses.Count; i++)
                    {
                        if (_computeProcesses.Peek().Item2 == port)
                            break;
                        var item = _computeProcesses.Dequeue();
                        _computeProcesses.Enqueue(item);
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
            // _pendingSpawnPorts tracks ports currently being started in background tasks so
            // we do not double-reserve them or exceed SpawnCount.
            int port;
            lock (_lockObject)
            {
                if (_computeProcesses.Count + _pendingSpawnPorts.Count >= SpawnCount)
                    return;

                var usedPorts = new HashSet<int>(_computeProcesses.Select(t => t.Item2));
                usedPorts.UnionWith(_pendingSpawnPorts);
                port = FindFreePort(usedPorts);
                if (port == 0) return;
                _pendingSpawnPorts.Add(port);
            }

            // Start the process and wait outside the lock so that other threads can
            // continue serving requests through already-ready children while this one loads.
            if (!TryStartChild(pathToCompute, port, out var process))
            {
                Log.Warning("compute.geometry on port {Port} failed to start within 60 seconds", port);
                lock (_lockObject) { _pendingSpawnPorts.Remove(port); }
                return;
            }

            lock (_lockObject)
            {
                _pendingSpawnPorts.Remove(port);
                if (process != null && !process.HasExited)
                    _computeProcesses.Enqueue(Tuple.Create(process, port));
            }
        }

        // compute.geometry is allowed to be in a sibling or child directory named compute.geometry.
        // Returns null if the executable cannot be found.
        static string FindComputeExecutablePath()
        {
            var pathToThisAssembly = new System.IO.FileInfo(typeof(ComputeChildren).Assembly.Location);
            var parentDirectory = pathToThisAssembly.Directory.Parent;

            string path = System.IO.Path.Combine(parentDirectory.FullName, "compute.geometry", "compute.geometry");
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                path += ".exe";

            return System.IO.File.Exists(path) ? path : null;
        }

        // Shared helper: creates the start info, starts the process on the given port, and waits
        // for the port to open. Returns true on success, false if the port never opened (timeout).
        static bool TryStartChild(string pathToCompute, int port, out Process process)
        {
            var startInfo = CreateComputeStartInfo(pathToCompute, port);
            process = Process.Start(startInfo);
            return WaitForChildProcess(process, port);
        }

        // Returns the first port >= 6001 that is not in usedPorts and is not already listening.
        // Returns 0 if no free port is found.
        static int FindFreePort(HashSet<int> usedPorts)
        {
            for (int i = 0; i < 256; i++)
            {
                if (i == 255) return 0;
                int port = 6001 + i;
                if (usedPorts.Contains(port)) continue;
                if (IsPortOpen(port)) continue;
                return port;
            }
            return 0;
        }

        static ProcessStartInfo CreateComputeStartInfo(string pathToCompute, int port)
        {
            var startInfo = new ProcessStartInfo(pathToCompute);
            startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "";
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
                if (IsPortOpen(port))
                    return true;

                if ((DateTime.Now - start).TotalSeconds > timeoutSeconds)
                {
                    process?.Kill();
                    return false;
                }

                Thread.Sleep(1000);
            }
        }

        // Returns true if any TCP listener is currently bound to the given port.
        // Uses IPGlobalProperties to enumerate OS-level listeners without opening a socket,
        // avoiding any SocketException throws regardless of platform.
        static bool IsPortOpen(int port)
        {
            var listeners = System.Net.NetworkInformation.IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners();
            return Array.Exists(listeners, ep => ep.Port == port);
        }
        static object _lockObject = new object();
        static Queue<Tuple<Process, int>> _computeProcesses = new Queue<Tuple<Process, int>>();
        // Ports for which a child process has been started but has not yet been confirmed
        // ready and added to _computeProcesses. Protected by _lockObject.
        static HashSet<int> _pendingSpawnPorts = new HashSet<int>();
    }
}
