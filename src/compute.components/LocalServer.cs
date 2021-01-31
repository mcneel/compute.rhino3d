using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compute.Components
{
    /// <summary>
    /// Utility for managing local compute server instances. When Hops
    /// components are referencing paths to files (instead of http URLs),
    /// the definitions are processed by compute server instances running
    /// on the same machine. Hops ships with a copy of compute.geometry.exe
    /// that it can launch when a compute server is needed.
    /// </summary>
    class LocalServer
    {
        public static int ActiveComputeCount
        {
            get
            {
                var processes = Process.GetProcessesByName("compute.geometry");
                return processes.Length;
            }
        }

        public static string GetDescriptionUrl(string definitionPath)
        {
            string baseUrl = GetComputeServerBaseUrl();
            return $"{baseUrl}/io?pointer={System.Web.HttpUtility.UrlEncode(definitionPath)}";
        }

        public static string GetSolveUrl()
        {
            string baseUrl = GetComputeServerBaseUrl();
            return baseUrl + "/grasshopper";
        }

        /// <summary>
        /// Get base url for a compute server. This function may return a
        /// different string each time it is called as it attempts to provide
        /// basic round robin scheduling when multiple compute servers are
        /// found to be available.
        /// </summary>
        /// <returns></returns>
        static string GetComputeServerBaseUrl()
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

                    // see if any compute.geometry process are already open
                    var processes = Process.GetProcessesByName("compute.geometry");
                    foreach (var process in processes)
                    {
                        int port = 8081;
                        var chunks = process.MainWindowTitle.Split(new char[] { ':' });
                        if (chunks.Length > 1)
                        {
                            port = int.Parse(chunks[1]);
                        }
                        var item = Tuple.Create(process, port);
                        _computeProcesses.Enqueue(item);
                    }

                    if (_computeProcesses.Count == 0)
                    {
                        LaunchCompute(_computeProcesses, true);
                    }

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

            return $"http://localhost:{activePort}";
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
            string pathToGha = typeof(LocalServer).Assembly.Location;
            string dir = System.IO.Path.GetDirectoryName(pathToGha);
            string pathToCompute = System.IO.Path.Combine(dir, "compute", "compute.geometry.exe");
            if (!System.IO.File.Exists(pathToCompute))
                return;

            var queueAsList = processQueue.ToArray();

            var existingProcesses = Process.GetProcessesByName("compute.geometry");
            var existingPorts = new HashSet<int>();
            foreach (var existingProcess in existingProcesses)
            {
                bool checkTitle = true;
                // see if this process is already in the queue
                foreach(var item in processQueue)
                {
                    if (existingProcess.Id == item.Item1.Id)
                    {
                        existingPorts.Add(item.Item2);
                        checkTitle = false;
                        break;
                    }
                }

                if (checkTitle)
                {
                    var chunks = existingProcess.MainWindowTitle.Split(new char[] { ':' });
                    if (chunks.Length > 1)
                    {
                        if (int.TryParse(chunks[chunks.Length - 1], out int lookForPort))
                        {
                            existingPorts.Add(lookForPort);
                        }
                    }
                }
            }
            int port = 0;
            for(int i=0;i<256; i++)
            {
                // start at port 6000. Feel free to change this if there is a reason
                // to use a different port
                port = 6000 + i;
                if (existingPorts.Contains(port))
                    continue;

                if (i == 255)
                    return;

                bool isOpen = IsPortOpen("localhost", port, new TimeSpan(0, 0, 0, 0, 100));
                if (isOpen)
                    continue;

                break;
            }

            var startInfo = new ProcessStartInfo(pathToCompute);
            startInfo.Arguments = $"-port:{port} -childof:{Process.GetCurrentProcess().Id}";
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.CreateNoWindow = true;
            var process = Process.Start(startInfo);
            var start = DateTime.Now;

            /*
            while (true)
            {
                System.Threading.Thread.Sleep(500);
                // It looks like .NET caches the window title for a Process instance.
                // The following hack is to work around the fact that we are changing
                // the title to relay information about the port used
                var temp = Process.GetProcessById(process.Id);
                string title = temp.MainWindowTitle;
                var chunks = title.Split(new char[] { ':' });
                if (chunks.Length > 1)
                {
                    string sPort = chunks[chunks.Length - 1];
                    if(int.TryParse(sPort, out int computePort))
                    {
                        if (computePort == port)
                            break;
                    }
                }
                var span = DateTime.Now - start;
                if (span.TotalSeconds > 20)
                {
                    process.Kill();
                    throw new Exception("Unable to start a local compute server");
                }
            }
            */

            if (waitUntilServing)
            {
                while (true)
                {
                    bool isOpen = IsPortOpen("localhost", port, new TimeSpan(0, 0, 1));
                    if (isOpen)
                        break;
                    var span = DateTime.Now - start;
                    if (span.TotalSeconds > 20)
                    {
                        process.Kill();
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
            catch
            {
                return false;
            }
        }
        static object _lockObject = new Object();
        static Queue<Tuple<Process, int>> _computeProcesses = new Queue<Tuple<Process, int>>();
    }
}
