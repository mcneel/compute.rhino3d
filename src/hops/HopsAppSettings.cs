using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper.GUI;

namespace Hops
{
    static class HopsAppSettings
    {
        const string HOPS_SERVERS = "Hops:Servers";
        const string HOPS_APIKEY = "Hops:ApiKey";
        const string HOPS_HTTP_TIMEOUT = "Hops:HttpTimeout";
        const string HIDE_WORKER_WINDOWS = "Hops:HideWorkerWindows";
        const string LAUNCH_WORKER_AT_START = "Hops:LaunchWorkerAtStart";
        const string LOCAL_WORKER_COUNT = "Hops:LocalWorkerCount";
        //const string SYNCHRONOUS_WAIT_TIME = "Hops:SynchronousWaitTime";
        const string MAX_CONCURRENT_REQUESTS = "Hops:MaxConcurrentRequests";
        const string RECURSION_LIMIT = "Hops:RecursionLimit";
        const string HOPS_FUNCTION_PATHS = "Hops:FunctionPaths";
        const string HOPS_FUNCTION_NAMES = "Hops:FunctionNames";
        const string HOPS_FUNCTION_SELECTED_STATE = "Hops:FunctionSelectedState";
        
        public static List<FunctionSourceRow> FunctionSources { get; set; } = new List<FunctionSourceRow>();

        public static bool ShowFunctionManager { get; set; } = true;

        public static void CheckFunctionManagerStatus()
        {
            var ver = typeof(Rhino.RhinoApp).Assembly.GetName().Version;
            if (Rhino.Runtime.HostUtils.RunningOnOSX)
            {
                if ((ver.Major >= 8 && ver.Build >= 22126 )|| (ver.Major == 7 && ver.Minor >= 19))
                {
                    ShowFunctionManager = true;
                }
                else
                {
                    ShowFunctionManager = false;
                    return;
                }
            }
            ShowFunctionManager = true;
        }

        public static string[] Servers
        {
            get
            {
                string serversSetting = Grasshopper.Instances.Settings.GetValue(HOPS_SERVERS, "");
                if (string.IsNullOrWhiteSpace(serversSetting))
                    return new string[0];
                var servers = serversSetting.Split(new char[] { '\n' });
                return servers;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_SERVERS, "");
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < value.Length; i++)
                    {
                        string s = value[i].Trim();
                        if (string.IsNullOrEmpty(s))
                            continue;
                        if (sb.Length > 0)
                            sb.Append('\n');
                        sb.Append(s);
                    }
                    Grasshopper.Instances.Settings.SetValue(HOPS_SERVERS, sb.ToString());
                }
                Hops.Servers.SettingsChanged();
            }
        }

        public static string APIKey
        {
            get
            {
                string apiKey = Grasshopper.Instances.Settings.GetValue(HOPS_APIKEY, "");
                if (string.IsNullOrWhiteSpace(apiKey))
                    return String.Empty;
                return apiKey;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_APIKEY, "");
                }
                else
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_APIKEY, value);
                }
            }
        }

        public static void InitFunctionSources()
        {
            if (FunctionSourcePaths.Length != FunctionSourceNames.Length && FunctionSourcePaths.Length != FunctionSourceSelectedStates.Length)
                return;
            if (FunctionSources == null)
                FunctionSources = new List<FunctionSourceRow>();
            if(FunctionSources.Count > 0)
                FunctionSources.Clear();
            for(int i = 0; i < FunctionSourcePaths.Length; i++)
            {
                var row = new FunctionSourceRow(FunctionSourceNames[i].Trim(), FunctionSourcePaths[i].Trim());
                FunctionSources.Add(row);
                bool isChecked;
                if (Boolean.TryParse(FunctionSourceSelectedStates[i], out isChecked))
                    FunctionSources[i].RowCheckbox.Checked = isChecked;
            }
        }

        public static string[] FunctionSourcePaths
        {
            get
            {
                string pathSetting = Grasshopper.Instances.Settings.GetValue(HOPS_FUNCTION_PATHS, "");
                if (string.IsNullOrWhiteSpace(pathSetting))
                    return new string[0];
                var paths = pathSetting.Split(new char[] { '\n' });
                return paths;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_PATHS, "");
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < value.Length; i++)
                    {
                        string s = value[i].Trim();
                        if (string.IsNullOrEmpty(s))
                            continue;
                        if (sb.Length > 0)
                            sb.Append('\n');
                        sb.Append(s);
                    }
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_PATHS, sb.ToString());
                }
            }
        }

        public static string[] FunctionSourceNames
        {
            get
            {
                string nameSetting = Grasshopper.Instances.Settings.GetValue(HOPS_FUNCTION_NAMES, "");
                if (string.IsNullOrWhiteSpace(nameSetting))
                    return new string[0];
                var names = nameSetting.Split(new char[] { '\n' });
                return names;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_NAMES, "");
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < value.Length; i++)
                    {
                        string s = value[i].Trim();
                        if (string.IsNullOrEmpty(s))
                            continue;
                        if (sb.Length > 0)
                            sb.Append('\n');
                        sb.Append(s);
                    }
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_NAMES, sb.ToString());
                }
            }
        }

        public static string[] FunctionSourceSelectedStates
        {
            get
            {
                string selectedSetting = Grasshopper.Instances.Settings.GetValue(HOPS_FUNCTION_SELECTED_STATE, "");
                if (string.IsNullOrWhiteSpace(selectedSetting))
                    return new string[0];
                var selections = selectedSetting.Split(new char[] { '\n' });
                return selections;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_SELECTED_STATE, "");
                }
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < value.Length; i++)
                    {
                        string s = value[i].Trim();
                        if (string.IsNullOrEmpty(s))
                            continue;
                        if (sb.Length > 0)
                            sb.Append('\n');
                        sb.Append(s);
                    }
                    Grasshopper.Instances.Settings.SetValue(HOPS_FUNCTION_SELECTED_STATE, sb.ToString());
                }
            }
        }

        public static bool HideWorkerWindows
        {
            get
            {
                bool show = Grasshopper.Instances.Settings.GetValue(HIDE_WORKER_WINDOWS, true);
                return show;
            }
            set
            {
                Grasshopper.Instances.Settings.SetValue(HIDE_WORKER_WINDOWS, value);
            }
        }

        public static bool LaunchWorkerAtStart
        {
            get
            {
                bool show = Grasshopper.Instances.Settings.GetValue(LAUNCH_WORKER_AT_START, true);
                return show;
            }
            set
            {
                Grasshopper.Instances.Settings.SetValue(LAUNCH_WORKER_AT_START, value);
            }
        }

        public static int LocalWorkerCount
        {
            get
            {
                int count = Grasshopper.Instances.Settings.GetValue(LOCAL_WORKER_COUNT, 1);
                return count;
            }
            set
            {
                if (value >= 0)
                    Grasshopper.Instances.Settings.SetValue(LOCAL_WORKER_COUNT, value);
            }
        }

        public static int RecursionLimit
        {
            get
            {
                int limit = Grasshopper.Instances.Settings.GetValue(RECURSION_LIMIT, 10);
                return limit;
            }
            set
            {
                if (value >= 0)
                    Grasshopper.Instances.Settings.SetValue(RECURSION_LIMIT, value);
            }
        }

        static int _httpTimeout = 0;
        public static int HTTPTimeout
        {
            get
            {
                if (0 == _httpTimeout)
                    _httpTimeout = Grasshopper.Instances.Settings.GetValue(HOPS_HTTP_TIMEOUT, 100);
                return _httpTimeout;
            }
            set
            {
                if (value >= 1)
                {
                    Grasshopper.Instances.Settings.SetValue(HOPS_HTTP_TIMEOUT, value);
                    _httpTimeout = value;
                }
            }

        }

        //static int _waittime;
        //public static int SynchronousWaitTime
        //{
        //    get
        //    {
        //        if (0==_waittime)
        //            _waittime = Grasshopper.Instances.Settings.GetValue(SYNCHRONOUS_WAIT_TIME, 50);
        //        return _waittime;
        //    }
        //    set
        //    {
        //        if (value >= 0)
        //        {
        //            Grasshopper.Instances.Settings.SetValue(SYNCHRONOUS_WAIT_TIME, value);
        //            _waittime = value;
        //        }
        //    }
        //}

        static int _maxConcurrentRequests = 0;
        public static int MaxConcurrentRequests
        {
            get
            {
                if (0 == _maxConcurrentRequests)
                    _maxConcurrentRequests = Grasshopper.Instances.Settings.GetValue(MAX_CONCURRENT_REQUESTS, 4);
                return _maxConcurrentRequests;
            }
            set
            {
                if (value >= 1)
                {
                    Grasshopper.Instances.Settings.SetValue(MAX_CONCURRENT_REQUESTS, value);
                    _maxConcurrentRequests = value;
                }
            }

        }
    }
    /*
    public class HopsAddSettingsCategory : IGH_SettingCategory
    {
        public string Parent => "";

        public string Name => "Hops";

        public string Description => "Settings for Hops and remote servers";

        public Bitmap Icon
        {
            get
            {
                var stream = GetType().Assembly.GetManifestResourceStream("Hops.resources.Hops_48x48.png");
                return new System.Drawing.Bitmap(stream);
            }
        }
    }
    */

    public class HopsAppSettingsFrontEnd : IGH_SettingFrontend
    {
        public string Category => "Solver";

        public string Name => "Hops - Compute server URLs";

        public IEnumerable<string> Keywords => new string[] { "Hops" };

        public Control SettingsUI()
        {
            return new HopsAppSettingsUserControl();
        }
    }
}
