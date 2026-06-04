using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.GUI;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using System.Drawing;
using System.Reflection;
using Grasshopper;
using System.IO;
using Grasshopper.Kernel;
using Newtonsoft.Json;

namespace Hops
{
    public static class HopsFunctionMgr
    {
        static ThumbnailViewer Viewer { get; set; }

        static HopsFunctionMgr()
        {
            if (Viewer == null)
                Viewer = new ThumbnailViewer();
            Viewer.Owner = Instances.DocumentEditor;
            Viewer.StartPosition = FormStartPosition.Manual;
            Viewer.Visible = false;
        }

        public static void SeekFunctionMenuDirs(UriFunctionPathInfo path, string uri, string fullpath, FunctionSourceRow row)
        {
            if (path == null)
                return;

            if (String.IsNullOrEmpty(uri))
                return;

            var endpoints = uri.Split(new[] { '/' }, 2);

            if (!String.IsNullOrEmpty(endpoints[1]))
            {
                if (endpoints[1].Contains("/"))
                {
                    var subendpoints = endpoints[1].Split(new[] { '/' }, 2);
                    UriFunctionPathInfo functionPath = new UriFunctionPathInfo("/" + subendpoints[0], true);
                    functionPath.RootURL = row.SourcePath;
                    path.Paths.Add(functionPath);
                    SeekFunctionMenuDirs(functionPath, "/" + subendpoints[1], fullpath, row);
                }
                else
                {
                    UriFunctionPathInfo functionPath = new UriFunctionPathInfo("/" + endpoints[1], false);
                    functionPath.RootURL = row.SourcePath;
                    functionPath.FullPath = fullpath;
                    path.Paths.Add(functionPath);
                }
            }
        }

        public static void SeekFunctionMenuDirs(FunctionPathInfo path)
        {
            if (path == null || !path.IsValid())
                return;

            string[] files = Directory.GetFiles(path.FullPath);
            foreach(string file in files)
            {
                FunctionPathInfo filePath = new FunctionPathInfo(file, false);
                path.Paths.Add(filePath);
            }

            string[] subDirs = Directory.GetDirectories(path.FullPath);
            foreach (string subDir in subDirs)
            {
                FunctionPathInfo subDirPath = new FunctionPathInfo(subDir, true);
                path.Paths.Add(subDirPath);
                SeekFunctionMenuDirs(subDirPath);
            }
        }

        internal static void tsm_HoverEnter(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem))
                return;
            ToolStripMenuItem ti = sender as ToolStripMenuItem;

            var thumbnail = GH_DocumentIO.GetDocumentThumbnail(ti.Name);
            if (Viewer != null && thumbnail != null && ti.Owner != null && Rhino.Runtime.HostUtils.RunningOnWindows)
            {
                var point = ti.Owner.PointToScreen(new Point(ti.Width + 4, 0));
                Viewer.Location = point;
                Viewer.pictureBox.Image = thumbnail;
                Viewer.Show();
            }
        }

        internal static void tsm_HoverExit(object sender, EventArgs e)
        {
            if (Viewer != null && Viewer.Visible)
            {
                Viewer.Hide();
            }
        }

        static Image settingsIcon;
        static Image statusOkIcon;
        static Image statusErrorIcon;
        static Image statusWarningIcon;
        static Image statusNoneIcon;

        public static Image SettingsIcon()
        {
            if (settingsIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Settings_96x96.png");
                settingsIcon = Image.FromStream(stream);
            }
            return settingsIcon;
        }
        public static Image StatusOkIcon()
        {
            if (statusOkIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.OK_24x24.png");
                statusOkIcon = Image.FromStream(stream);
            }
            return statusOkIcon;
        }
        public static Image StatusErrorIcon()
        {
            if (statusErrorIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Error_24x24.png");
                statusErrorIcon = Image.FromStream(stream);
            }
            return statusErrorIcon;
        }
        public static Image StatusWarningIcon()
        {
            if (statusWarningIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Warning_24x24.png");
                statusWarningIcon = Image.FromStream(stream);
            }
            return statusWarningIcon;
        }
        public static Image StatusNoneIcon()
        {
            if (statusNoneIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.None_24x24.png");
                statusNoneIcon = Image.FromStream(stream);
            }
            return statusNoneIcon;
        }
        static System.Net.Http.HttpClient httpClient = null;
        public static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                {
                    // Per-request deadlines come from the call site's CancellationTokenSource
                    // so HopsAppSettings.HttpTimeout values larger than 100s aren't capped here.
                    httpClient = new System.Net.Http.HttpClient
                    {
                        Timeout = System.Threading.Timeout.InfiniteTimeSpan
                    };
                }
                return httpClient;
            }
        }
    }
}
