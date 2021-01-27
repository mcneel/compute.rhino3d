using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compute.Components
{
    class LocalServer
    {
        public static string GetDescriptionUrl(string definitionPath)
        {
            int port = GetComputeServerPort();
            return $"http://localhost:{port}/io?pointer={System.Web.HttpUtility.UrlEncode(definitionPath)}";
        }

        public static string GetSolveUrl()
        {
            int port = GetComputeServerPort();
            return $"http://localhost:{port}/grasshopper";
        }

        static int GetComputeServerPort()
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
            return activePort;
        }

        static Queue<Tuple<Process, int>> _computeProcesses = new Queue<Tuple<Process, int>>();
    }
}
