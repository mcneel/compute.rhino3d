using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Hops
{
    /// <summary>
    /// Utility for managing compute server instances. When Hops components are
    /// referencing paths to files (instead of http URLs), the definitions are
    /// processed by compute server instances running local or remote. Hops
    /// ships with rhino.compute.exe and compute.geometry.exe that it can launch
    /// when a compute server is needed.
    /// </summary>
    class Servers
    {
        static System.Threading.Tasks.Task<bool> _initServerTask;
        public static void StartServerOnLaunch()
        {
            _initServerTask = System.Threading.Tasks.Task.Run(() =>
            {
                lock (_lockObject)
                {
                    LaunchLocalRhinoCompute(_computeServerQueue, true);
                }
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
                    if (_computeServerQueue.Count == 0)
                    {
                        LaunchLocalRhinoCompute(_computeServerQueue, true);
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

        public static void LaunchChildComputeGeometry(int childCount)
        {
            if (childCount < 1)
                return;
            string baseUrl = GetComputeServerBaseUrl();
            int thisProc = Process.GetCurrentProcess().Id;
            string address = $"{baseUrl}/launch?children={childCount}&parent={thisProc}";
            RemoteDefinition.HttpClient.GetAsync(address);
        }

        const int RhinoComputePort = 6500;

        /// <summary>
        /// Launch rhino.compute reverse proxy server on this computer
        /// </summary>
        /// <param name="serverQueue"></param>
        /// <param name="waitUntilServing"></param>
        static void LaunchLocalRhinoCompute(Queue<ComputeServer> serverQueue, bool waitUntilServing)
        {
            // There is one and only one local rhino.compute server ever needed
            // on a single computer. Check the serverQueue to make sure we don't
            // alreay have one
            foreach(var server in serverQueue.ToArray())
            {
                if (server.IsLocalProcess)
                    return;
            }

            var existingProcesses = Process.GetProcessesByName("rhino.compute");
            if( existingProcesses!=null && existingProcesses.Length>0)
            {
                if (IsPortOpen("localhost", RhinoComputePort, new TimeSpan(0, 0, 0, 0, 100)))
                {
                    serverQueue.Enqueue(new ComputeServer(existingProcesses[0], RhinoComputePort));
                }
            }

            // No rhino.compute.exe running. Launch one
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
            string pathToRhinoCompute = System.IO.Path.Combine(dir, "rhino.compute", "rhino.compute.exe");
            if (!System.IO.File.Exists(pathToRhinoCompute))
            {
                // debug builds are in net5.0 directory
                pathToRhinoCompute = System.IO.Path.Combine(dir, "net5.0", "rhino.compute.exe");
                if (!System.IO.File.Exists(pathToRhinoCompute))
                    return;
            }

            var startInfo = new ProcessStartInfo(pathToRhinoCompute);
            int childCount = Hops.HopsAppSettings.LocalWorkerCount;
            if (childCount < 1)
                childCount = 1;
            int thisProc = Process.GetCurrentProcess().Id;
            startInfo.Arguments = $"--childof {thisProc} --childcount {childCount} --port {RhinoComputePort} --spawn-on-startup";
            startInfo.WindowStyle = Hops.HopsAppSettings.HideWorkerWindows ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Minimized;
            // uncomment next line to ease debugging
            // startInfo.WindowStyle = ProcessWindowStyle.Normal;
            // Important: Keep UseShellExecute = true
            // If UseShellExecute is false, the child process inherits file handles from
            // the parent process. We found this out by noticing that Rhino suddenly thought
            // all of it's files were read-only. If we want to go with UseShellExecute = false
            // then we need to write a pInvoke to CreateProcess with the copyFileHandles
            // set to false.
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = Hops.HopsAppSettings.HideWorkerWindows;
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 6 April 2022 - S. Baer (COMPUTE-241)
            // When grasshopper memory loads assemblies, the above line results in an
            // empty string. In this case use the assemblyinfo location property.
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                assemblyPath = GhaAssemblyInfo.TheAssemblyInfo.Location;
            }
            string parentPath = Path.GetDirectoryName(assemblyPath);
            startInfo.WorkingDirectory = Path.Combine(parentPath, "rhino.compute");
            var process = Process.Start(startInfo);
            var start = DateTime.Now;

            if (waitUntilServing)
            {
                while (true)
                {
                    bool isOpen = IsPortOpen("localhost", RhinoComputePort, new TimeSpan(0, 0, 1));
                    if (isOpen)
                        break;
                    var span = DateTime.Now - start;
                    // If rhino.compute takes more than 60 seconds to launch, assume something
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
                serverQueue.Enqueue(new ComputeServer(process, RhinoComputePort));
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
