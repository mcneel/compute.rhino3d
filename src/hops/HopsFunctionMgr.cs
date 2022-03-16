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
        static ThumbnailViewer Viewer { get; set; }
        public static void AddFunctionMgrControl()
        {
            Type editorType = typeof(Grasshopper.GUI.GH_DocumentEditor);
            if(editorType != null)
            {
                BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
                FieldInfo field = editorType.GetField("_CanvasToolbar", binding);
                if(field != null && Instances.DocumentEditor != null)
                {
                    try
                    {
                        object fieldInstance = field.GetValue(Instances.DocumentEditor);
                        ToolStrip canvasToolbar = fieldInstance as ToolStrip;
                        if (canvasToolbar == null)
                            return;
                        var menuItem = CreateMenuDropDown();
                        canvasToolbar.Items.Add(menuItem);
                        InitThumbnailViewer();
                        Viewer.Location = new Point(canvasToolbar.Location.X, canvasToolbar.Location.Y + 24);
                    }
                    catch(Exception ex) { }
                }
            }
        }

        private static void InitThumbnailViewer()
        {
            if (Viewer == null)
                Viewer = new ThumbnailViewer();
            Viewer.Owner = Instances.DocumentEditor;
            Viewer.Visible = false;
        }

        private static ToolStripItem CreateMenuDropDown()
        {
            ToolStripMenuItem fileMgr = new ToolStripMenuItem(null, FuncMgr24Icon(), DropDownItemClicked, "Functions");
            return fileMgr;
        }

        private static void DropDownItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem fileMgr = sender as ToolStripMenuItem;
            if (fileMgr != null)
            {
                fileMgr.DropDownItems.Clear();
                GenerateFunctionPathMenu(fileMgr);
            }
        }

        private static void GenerateFunctionPathMenu(ToolStripMenuItem menu)
        {
            if (!String.IsNullOrEmpty(HopsAppSettings.FunctionManagerRootPath))
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
            ToolStrip parent = ti.GetCurrentParent();
            var editor = Instances.DocumentEditor;
            Control canvasToolbar = null;
            foreach(Control control in editor.Controls)
            {
                if (control.Name == "Panel1")
                {
                    canvasToolbar = control;
                    break;
                }   
            }
   
            var thumbnail = GH_DocumentIO.GetDocumentThumbnail(ti.Name);
            Size offset = new Size(156, 32);
            if (Viewer != null && thumbnail != null && canvasToolbar != null)
            {
                var point = canvasToolbar.PointToScreen(new Point(offset.Width, offset.Height));
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

            if (Instances.ActiveCanvas != null && !String.IsNullOrEmpty(ti.Name))
            {
                Grasshopper.Kernel.GH_Document doc;
                if (Instances.ActiveCanvas.Document == null)
                {
                    doc = Instances.DocumentServer.AddNewDocument();
                    Instances.ActiveCanvas.Document = doc;
                }
                else
                    doc = Instances.ActiveCanvas.Document;

                switch (e.Button)
                {
                    case MouseButtons.Left:
                        HopsComponent hops = new HopsComponent();

                        RectangleF boxTarget = Instances.ActiveCanvas.Viewport.VisibleRegion;
                        PointF pointTarget = new PointF(boxTarget.X + 0.5f * boxTarget.Width, boxTarget.Y + 0.5f * boxTarget.Height);
                        hops.Attributes.Pivot = pointTarget;
                        hops.RemoteDefinitionLocation = ti.Name;

                        doc.AddObject(hops, true);
                        doc.ExpireSolution();
                        doc.UndoUtil.RecordAddObjectEvent("Hops", hops);

                        break;
                    case MouseButtons.Right:
                        try
                        { 
                            Instances.DocumentEditor.ScriptAccess_OpenDocument(ti.Name);
                        }
                        catch(Exception ex) { }
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
