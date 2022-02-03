using System;
using System.Collections.Generic;

namespace compute.geometry
{
    class Shutdown
    {
        static System.Threading.Timer _timer;
        internal static Dictionary<int, System.Diagnostics.Process> ParentProcesses { get; set; }
        static int _parentPort = -1;
        static int _idleSpan = -1;
        static DateTime _startTime;

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

        public static void RegisterStartTime(DateTime dateTime)
        {
            _startTime = dateTime;
        }

        public static void RegisterParentPort(int port)
        {
            _parentPort = port;
        }

        public static void RegisterIdleSpan(int spanSeconds)
        {
            _idleSpan = spanSeconds;
        }

        public static void StartTimer(Topshelf.HostControl hctrl)
        {
            bool startTimer = false;
            if (_timer == null && (ParentProcesses != null))
            {
                startTimer = true;
            }
            if (_timer == null && _idleSpan > 0 && _parentPort > 0)
            {
                startTimer = true;
            }

            if (startTimer)
            {
                _timer = new System.Threading.Timer(
                    new System.Threading.TimerCallback(TimerTask), hctrl, 1000, 2000);
            }
        }

        static System.Net.Http.HttpClient _httpClient;
        static DateTime _lastSpanCheck = DateTime.Now;

        private static void TimerTask(object timerState)
        {
            bool shutdown = false;

            if (ParentProcesses != null)
            {
                shutdown = true;
                foreach (var process in ParentProcesses.Values)
                {
                    if (!process.HasExited)
                        shutdown = false;
                }
            }

            if (!shutdown && _parentPort > 0 && _idleSpan > 0)
            {
                // Don't check the server every timer tick. Just check when we start approaching
                // what we think is our span limit.
                var shouldCheckSpan = DateTime.Now - _lastSpanCheck;
                if (shouldCheckSpan.TotalSeconds * 0.5 > _idleSpan)
                {
                    if (_httpClient == null)
                        _httpClient = new System.Net.Http.HttpClient();
                    string url = $"http://localhost:{_parentPort}/idlespan";
                    _lastSpanCheck = DateTime.Now;
                    try
                    {
                        string span = _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                        int serverIdleSpan = int.Parse(span);
                        if (serverIdleSpan > _idleSpan)
                            shutdown = true;
                    }
                    catch(Exception)
                    {
                        // not sure what to do here
                    }
                }
            }


            if (shutdown)
            {
                var elapsedTime = DateTime.Now - _startTime;
                //Serilog.Log.Information($"Child process stopped at " + DateTime.Now.ToLocalTime().ToString());
                Serilog.Log.Information("Total elapsed time for child process is " + string.Format("{0:D2} days, {1:D2} hrs, {2:D2} mins, {3:D2} secs", elapsedTime.Days, elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds));
                var hctrl = timerState as Topshelf.HostControl;
                if (hctrl != null)
                    hctrl.Stop();
            }
        }
    }
}
