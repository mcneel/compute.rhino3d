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

namespace Hops
{
    public static class HopsFunctionMgr
    {
        private static HopsComponent Parent { get; set; }
        static ThumbnailViewer Viewer { get; set; }
        public static ToolStripMenuItem AddFunctionMgrControl(HopsComponent _parent)
        {
            Parent = _parent;
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Available Functions", null, null, "Available Functions");
            menuItem.DropDownItems.Clear();
            GenerateFunctionPathMenu(menuItem);
            InitThumbnailViewer();
            return menuItem;
        }

        private static void InitThumbnailViewer()
        {
            if (Viewer == null)
                Viewer = new ThumbnailViewer();
            Viewer.Owner = Instances.DocumentEditor;
            Viewer.StartPosition = FormStartPosition.Manual;
            Viewer.Visible = false;
        }

        private static void GenerateFunctionPathMenu(ToolStripMenuItem menu)
        {
            if (!String.IsNullOrEmpty(HopsAppSettings.FunctionManagerRootPath) && Directory.Exists(HopsAppSettings.FunctionManagerRootPath))
            {
                FunctionPathInfo functionPaths = new FunctionPathInfo(HopsAppSettings.FunctionManagerRootPath, true);
                functionPaths.isRoot = true;

                SeekFunctionMenuDirs(functionPaths);
                if (functionPaths.Paths.Count != 0)
                {
                    functionPaths.BuildMenus(menu, new MouseEventHandler(tsm_Click), new EventHandler(tsm_HoverEnter), new EventHandler(tsm_HoverExit));
                    functionPaths.RemoveEmptyMenuItems(menu, tsm_Click, tsm_HoverEnter, tsm_HoverExit);
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

            if (Viewer != null && thumbnail != null && ti.Owner != null)
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

        static void tsm_Click(object sender, MouseEventArgs e)
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
                        catch (Exception ex) { }
                        break;
                }
            }
        }

        static Image _funcMgr24Icon;
        static Image _funcMgr48Icon;
        static Image FuncMgr24Icon()
        {
            if (_funcMgr24Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_Function_Mgr_24x24.png");
                _funcMgr24Icon = Image.FromStream(stream);
            }
            return _funcMgr24Icon;
        }
        static Image FuncMgr48Icon()
        {
            if (_funcMgr48Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_Function_Mgr_48x48.png");
                _funcMgr48Icon = Image.FromStream(stream);
            }
            return _funcMgr48Icon;
        }
    }
}
