using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compute.Components
{
    /// <summary>
    /// Utility for managing compute server instances. When Hops components are
    /// referencing paths to files (instead of http URLs), the definitions are
    /// processed by compute server instances running local or remote. Hops
    /// ships with a copy of compute.geometry.exe that it can launch when a
    /// compute server is needed.
    /// </summary>
    class Servers
    {
        static System.Threading.Tasks.Task<bool> _initServerTask;
        public static void StartServerOnLaunch()
        {
            _initServerTask = System.Threading.Tasks.Task.Run(() =>
            {
                LaunchLocalCompute(true);
                return true;
            });
        }

        public static void SettingsChanged()
        {
            _settingsNeedReading = true;
        }

        /// <summary>Number of local compute.geometry processes running</summary>
        public static int ActiveLocalComputeCount
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

        public static string GetDescriptionPostUrl()
        {
            string baseUrl = GetComputeServerBaseUrl();
            return $"{baseUrl}/io";
        }

        public static string GetDescriptionUrl(Guid componentId)
        {
            if (componentId == Guid.Empty)
                return null;
            string baseUrl = GetComputeServerBaseUrl();
            return $"{baseUrl}/io?pointer={System.Web.HttpUtility.UrlEncode(componentId.ToString())}";
        }

        public static string GetSolveUrl()
        {
            string baseUrl = GetComputeServerBaseUrl();
            return baseUrl + "/grasshopper";
        }

        /// <summary>
        /// Get base url for a compute server. May return a different string
        /// each time it is called as it provides basic round robin scheduling
        /// when multiple compute servers are found to be available.
        /// </summary>
        /// <returns></returns>
        static string GetComputeServerBaseUrl()
        {
            if (_initServerTask != null)
            {
                _initServerTask.Wait();
                _initServerTask = null;
            }
            // Simple round robin scheduler using a queue of compute.geometry processes
            string url = null;

            lock (_lockObject)
            {
                // Check application level settings to see if there are remote
                // compute servers defined.
                if (_settingsNeedReading)
                {
                    _settingsNeedReading = false;
                    string[] servers = Hops.HopsAppSettings.Servers;
                    var serverArray = _computeServerQueue.ToArray();
                    _computeServerQueue.Clear();
                    foreach (var server in servers)
                    {
                        if (string.IsNullOrWhiteSpace(server))
                            continue;
                        _computeServerQueue.Enqueue(new ComputeServer(server));
                    }
                    foreach(var item in serverArray)
                    {
                        if (item.IsLocalProcess)
                            _computeServerQueue.Enqueue(item);
                    }
                }

                if (_computeServerQueue.Count > 0)
                {
                    var current = _computeServerQueue.Dequeue();
                    url = current.GetUrl();
                    if( !string.IsNullOrEmpty(url))
                    {
                        _computeServerQueue.Enqueue(current);
                    }
                }

                if (string.IsNullOrEmpty(url) && Rhino.Runtime.HostUtils.RunningOnWindows)
                {
                    _computeServerQueue = new Queue<ComputeServer>();

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
                        _computeServerQueue.Enqueue(new ComputeServer(process, port));
                    }

                    if (_computeServerQueue.Count == 0)
                    {
                        LaunchLocalCompute(_computeServerQueue, true);
                    }

                    if (_computeServerQueue.Count > 0)
                    {
                        var current = _computeServerQueue.Dequeue();
                        _computeServerQueue.Enqueue(current);
                        url = current.GetUrl();
                    }
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                string message = "No compute server found";
                if (Rhino.Runtime.HostUtils.RunningOnOSX)
                    message += ": Mac Rhino only supports external compute servers";
                throw new Exception(message);
            }

            return url;
        }

        public static void LaunchLocalCompute(bool waitUntilServing)
        {
            lock (_lockObject)
            {
                LaunchLocalCompute(_computeServerQueue, waitUntilServing);
            }
        }

        static void LaunchLocalCompute(Queue<ComputeServer> serverQueue, bool waitUntilServing)
        {
            string dir = null;
            if (GhaAssemblyInfo.TheAssemblyInfo != null)
            {
                dir = System.IO.Path.GetDirectoryName(GhaAssemblyInfo.TheAssemblyInfo.Location);
            }
            if (dir == null)
            {
                string pathToGha = typeof(Servers).Assembly.Location;
                dir = System.IO.Path.GetDirectoryName(pathToGha);
            }
            string pathToCompute = System.IO.Path.Combine(dir, "compute", "compute.geometry.exe");
            if (!System.IO.File.Exists(pathToCompute))
                return;

            var existingProcesses = Process.GetProcessesByName("compute.geometry");
            var existingPorts = new HashSet<int>();
            foreach (var existingProcess in existingProcesses)
            {
                bool checkTitle = true;
                // see if this process is already in the queue
                foreach(var item in serverQueue)
                {
                    if (item.IsProcess(existingProcess))
                    {
                        existingPorts.Add(item.LocalProcessPort());
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
            startInfo.WindowStyle = Hops.HopsAppSettings.HideWorkerWindows ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Minimized;
            // Important: Keep UseShellExecute = true
            // If UseShellExecute is false, the child process inherits file handles from
            // the parent process. We found this out by noticing that Rhino suddenly thought
            // all of it's files were read-only. If we want to go with UseShellExecute = false
            // then we need to write a pInvoke to CreateProcess with the copyFileHandles
            // set to false.
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = Hops.HopsAppSettings.HideWorkerWindows;
            var process = Process.Start(startInfo);
            var start = DateTime.Now;

            if (waitUntilServing)
            {
                while (true)
                {
                    bool isOpen = IsPortOpen("localhost", port, new TimeSpan(0, 0, 1));
                    if (isOpen)
                        break;
                    var span = DateTime.Now - start;
                    // If compute takes more than 60 seconds to launch, assume something
                    // is wrong and kill the process. I realize there are installs out
                    // there that take longer to load, but I don't have a better solution
                    // right now.
                    if (span.TotalSeconds > 60)
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
                serverQueue.Enqueue(new ComputeServer(process, port));
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
        static Queue<ComputeServer> _computeServerQueue = new Queue<ComputeServer>();
        static bool _settingsNeedReading = true;

        class ComputeServer
        {
            readonly Process _process;
            readonly int _port;
            readonly string _url;
            public ComputeServer(Process proc, int port)
            {
                _process = proc;
                _port = port;
                _url = null;
            }
            public ComputeServer(string url)
            {
                _process = null;
                _port = 0;
                _url = url.Trim(new char[] { '/' });
            }

            public bool IsLocalProcess
            {
                get { return _process != null; }
            }

            public bool IsProcess(Process proc)
            {
                if (_process == null)
                    return false;

                return _process.Id == proc.Id;
            }
            public int LocalProcessPort()
            {
                return _port;
            }

            public string GetUrl()
            {
                if(_process != null)
                {
                    if (_process.HasExited)
                        return null;
                    return $"http://localhost:{_port}";
                }
                return _url;
            }
        }
    }
}
