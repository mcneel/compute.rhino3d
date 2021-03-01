using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.GUI;

namespace Hops
{
    static class HopsAppSettings
    {
        const string ServersSetting = "Hops:Servers";
        public static string[] Servers
        {
            get
            {
                var servers = Grasshopper.Instances.Settings.GetValue(ServersSetting, "").Split(new char[] { '\n' });
                return servers;
            }
            set
            {
                if (value == null)
                {
                    Grasshopper.Instances.Settings.SetValue(ServersSetting, "");
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
                    Grasshopper.Instances.Settings.SetValue(ServersSetting, sb.ToString());
                }
                Compute.Components.Servers.SettingsChanged();
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

        public string Name => "URLs (Hops Compute Servers)";

        public IEnumerable<string> Keywords => new string[] { "Hops" };

        public Control SettingsUI()
        {
            return new HopsAppSettingsUserControl();
        }
    }
}
