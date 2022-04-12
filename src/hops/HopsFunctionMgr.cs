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
        private static HopsComponent Parent { get; set; }
        static ThumbnailViewer Viewer { get; set; }
        public static ToolStripMenuItem AddFunctionMgrControl(HopsComponent _parent)
        {
            HopsAppSettings.InitFunctionSources();
            if (HopsAppSettings.FunctionSources.Count <= 0)
                return null;
            Parent = _parent;
            ToolStripMenuItem mainMenu = new ToolStripMenuItem("Available Functions", null, null, "Available Functions");
            mainMenu.DropDownItems.Clear();
            foreach(var row in HopsAppSettings.FunctionSources)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(row.SourceName, null, null, row.SourceName);
                GenerateFunctionPathMenu(menuItem, row);
                mainMenu.DropDownItems.Add(menuItem);
            }
            InitThumbnailViewer();
            return mainMenu;
        }

        private static void InitThumbnailViewer()
        {
            if (Viewer == null)
                Viewer = new ThumbnailViewer();
            Viewer.Owner = Instances.DocumentEditor;
            Viewer.StartPosition = FormStartPosition.Manual;
            Viewer.Visible = false;
        }

        private static void GenerateFunctionPathMenu(ToolStripMenuItem menu, FunctionSourceRow row)
        {
            if (String.IsNullOrEmpty(row.SourceName) || String.IsNullOrEmpty(row.SourcePath)) 
                return;
            if (row.SourcePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var getTask = HttpClient.GetAsync(row.SourcePath);
                    if (getTask != null)
                    {
                        var responseMessage = getTask.Result;
                        var remoteSolvedData = responseMessage.Content;
                        var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(stringResult))
                        {
                            //invalid URL
                            return;
                        }
                        else
                        {
                            var response = JsonConvert.DeserializeObject<FunctionMgr_Schema[]>(stringResult);
                            if(response != null)
                            {
                                UriFunctionPathInfo functionPaths = new UriFunctionPathInfo(row.SourcePath, true);
                                functionPaths.isRoot = true;
                                functionPaths.RootURL = row.SourcePath;
                                if (!String.IsNullOrEmpty(response[0].Uri))
                                {
                                    //If the Schema Uri exists, then the response is likely from the ghhops_server.
                                    //Otherwise, let's assume the response is from the appserver
                                    foreach (FunctionMgr_Schema obj in response)
                                    {
                                        SeekFunctionMenuDirs(functionPaths, obj.Uri, obj.Uri, row);
                                    }
                                }
                                else if(!String.IsNullOrEmpty(response[0].Name))
                                {
                                    foreach (FunctionMgr_Schema obj in response)
                                    {
                                        SeekFunctionMenuDirs(functionPaths, "/" + obj.Name, "/" + obj.Name, row);
                                    }
                                }
                                if (functionPaths.Paths.Count != 0)
                                    functionPaths.BuildMenus(menu, new MouseEventHandler(tsm_UriClick));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            else if (Directory.Exists(row.SourcePath))
            {
                FunctionPathInfo functionPaths = new FunctionPathInfo(row.SourcePath, true);
                functionPaths.isRoot = true;

                SeekFunctionMenuDirs(functionPaths);
                if (functionPaths.Paths.Count != 0)
                {
                    functionPaths.BuildMenus(menu, new MouseEventHandler(tsm_FileClick), new EventHandler(tsm_HoverEnter), new EventHandler(tsm_HoverExit));
                    functionPaths.RemoveEmptyMenuItems(menu, tsm_FileClick, tsm_HoverEnter, tsm_HoverExit);
                }
            }
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

        static void tsm_HoverEnter(object sender, EventArgs e)
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

        static void tsm_HoverExit(object sender, EventArgs e)
        {
            if (Viewer != null && Viewer.Visible)
            {
                Viewer.Hide();
            }
        }

        static void tsm_FileClick(object sender, MouseEventArgs e)
        {
            if (!(sender is ToolStripItem))
                return;
            ToolStripItem ti = sender as ToolStripItem;
            
            if(Parent != null)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        Parent.RemoteDefinitionLocation = ti.Name;
                        if (Instances.ActiveCanvas.Document != null)
                            Instances.ActiveCanvas.Document.ExpireSolution();
                        break;
                    case MouseButtons.Right:
                        try
                        {
                            Instances.DocumentEditor.ScriptAccess_OpenDocument(ti.Name);
                        }
                        catch (Exception) { }
                        break;
                }
            }
        }

        static void tsm_UriClick(object sender, MouseEventArgs e)
        {
            if (!(sender is ToolStripItem))
                return;
            ToolStripItem ti = sender as ToolStripItem;

            if (Parent != null)
            {
                Parent.RemoteDefinitionLocation = ti.Tag as string;
                if (Instances.ActiveCanvas.Document != null)
                    Instances.ActiveCanvas.Document.ExpireSolution();
            }
        }

        static Image _funcMgr24Icon;
        static Image _funcMgr48Icon;
        static Image _deleteIcon;
        static Image _addIcon;
        static Image _editIcon;

        public static Image FuncMgr24Icon()
        {
            if (_funcMgr24Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_Function_Mgr_24x24.png");
                _funcMgr24Icon = Image.FromStream(stream);
            }
            return _funcMgr24Icon;
        }
        public static Image FuncMgr48Icon()
        {
            if (_funcMgr48Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_Function_Mgr_48x48.png");
                _funcMgr48Icon = Image.FromStream(stream);
            }
            return _funcMgr48Icon;
        }
        public static Image DeleteIcon()
        {
            if (_deleteIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Close_Toolbar_Active_20x20.png");
                _deleteIcon = Image.FromStream(stream);
            }
            return _deleteIcon;
        }
        public static Image AddIcon()
        {
            if (_addIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Open_Toolbar_Active_20x20.png");
                _addIcon = Image.FromStream(stream);
            }
            return _addIcon;
        }
        public static Image EditIcon()
        {
            if (_editIcon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.edit_16x16.png");
                _editIcon = Image.FromStream(stream);
            }
            return _editIcon;
        }
        static System.Net.Http.HttpClient _httpClient = null;
        public static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new System.Net.Http.HttpClient();
                }
                return _httpClient;
            }
        }
    }
}
