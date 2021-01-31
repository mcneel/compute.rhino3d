using System;
using System.Collections.Generic;

namespace compute.geometry
{
    class Shutdown
    {
        static System.Threading.Timer _timer;
        static Dictionary<int, System.Diagnostics.Process> ParentProcesses { get; set; }

        public static void RegisterParentProcess(int processId)
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);
            if (ParentProcesses == null)
            {
                ParentProcesses = new Dictionary<int, System.Diagnostics.Process>();
            }
            if (process != null)
                ParentProcesses[processId] = process;
        }

        public static void StartTimer(Topshelf.HostControl hctrl)
        {
            if ((ParentProcesses != null) && _timer == null)
            {
                _timer = new System.Threading.Timer(
                    new System.Threading.TimerCallback(TimerTask), hctrl, 1000, 2000);
            }
        }


        private static void TimerTask(object timerState)
        {
            if (ParentProcesses != null)
            {
                bool shutdown = true;
                foreach (var process in ParentProcesses.Values)
                {
                    if (!process.HasExited)
                        shutdown = false;
                }
                if (shutdown)
                {
                    var hctrl = timerState as Topshelf.HostControl;
                    if (hctrl != null)
                        hctrl.Stop();
                }
            }
        }
    }
}
