using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.GUI;

namespace Hops
{
    static class HopsAppSettings
    {
        const string HOPS_SERVERS = "Hops:Servers";
        const string HIDE_WORKER_WINDOWS = "Hops:HideWorkerWindows";
        const string LAUNCH_WORKER_AT_START = "Hops:LaunchWorkerAtStart";

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
                Compute.Components.Servers.SettingsChanged();
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
