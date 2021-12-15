using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;

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

        /// <summary>Port that rhino.compute is running on</summary>
        public static int ParentPort { get; set; } = 5000;
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
                    _computeProcesses = new Queue<Tuple<Process, int>>();
                    LaunchCompute(_computeProcesses, true);

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

            if (_computeProcesses.Count < SpawnCount)
            {
                // Bring up other child computes to SpawnCount level
                for(int i=_computeProcesses.Count; i<SpawnCount; i++)
                {
                    LaunchCompute(false);
                }
            }
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

        public static void LaunchCompute(bool waitUntilServing)
        {
            lock (_lockObject)
            {
                LaunchCompute(_computeProcesses, waitUntilServing);
            }
        }

        static void LaunchCompute(Queue<Tuple<Process, int>> processQueue, bool waitUntilServing)
        {
            Logger log = LogManager.GetCurrentClassLogger();

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

            string commandLineArgs = $"-port:{port} -childof:{Process.GetCurrentProcess().Id}";
            if (ParentPort > 0 && ChildIdleSpan.TotalSeconds > 1.0)
            {
                int seconds = (int)ChildIdleSpan.TotalSeconds;
                commandLineArgs += $" -parentport:{ParentPort} -idlespan:{seconds}";
            }
            startInfo.Arguments = commandLineArgs;

            var process = Process.Start(startInfo);
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
                        log.Debug("*********  Unable to start a local compute server. Timeout of 60 seconds was exceeded. *********");
                        throw new Exception("Unable to start a local compute server");
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
                log.Error("*********  Child process was started and is listening on port " + port.ToString() + "  *********");
                processQueue.Enqueue(Tuple.Create(process, port));
            }
        }


        static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            Logger log = LogManager.GetCurrentClassLogger();

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
            catch(Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }
        static object _lockObject = new object();
        static Queue<Tuple<Process, int>> _computeProcesses = new Queue<Tuple<Process, int>>();
    }
}
