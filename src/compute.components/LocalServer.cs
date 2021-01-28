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
            if(_computeProcesses.Count>0)
            {
                Tuple<Process, int> current = _computeProcesses.Dequeue();
                if(!current.Item1.HasExited)
                {
                    _computeProcesses.Enqueue(current);
                    activePort = current.Item2;
                }
            }

            if (activePort == 0)
            {
                _computeProcesses = new Queue<Tuple<Process, int>>();
                var processes = Process.GetProcessesByName("compute.geometry");
                foreach (var process in processes)
                {
                    int port = 8081;
                    var chunks = process.MainWindowTitle.Split(new char[] { ':' });
                    if (chunks.Length>1)
                    {
                        port = int.Parse(chunks[1]);
                    }
                    var item = Tuple.Create(process, port);
                    _computeProcesses.Enqueue(item);
                }

                if (_computeProcesses.Count > 0)
                {
                    Tuple<Process, int> current = _computeProcesses.Dequeue();
                    _computeProcesses.Enqueue(current);
                    activePort = current.Item2;
                }
            }

            if (0 == activePort)
                throw new Exception("No compute server found");

            return $"http://localhost:{activePort}";
        }

        static Queue<Tuple<Process, int>> _computeProcesses = new Queue<Tuple<Process, int>>();
    }
}
