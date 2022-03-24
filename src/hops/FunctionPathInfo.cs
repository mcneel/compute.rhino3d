using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;

namespace Hops
{
    public class FunctionPathInfo
    {
        public FunctionPathInfo(string _fullpath, bool _isfolder)
        {
            FullPath = _fullpath;
            IsFolder = _isfolder;
            if (IsFolder)
            {
                Extension = "";
                string[] str = FullPath.Split(Path.DirectorySeparatorChar);
                if (str != null && str.Length != 0)
                    FileName = str[str.Length - 1];
                else
                    FileName = Path.GetDirectoryName(FullPath);
            }
            else
            {
                Extension = Path.GetExtension(FullPath);
                FileName = Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public bool IsFolder { get; set; } = false;
        public bool isRoot { get; set; } = false;

        public bool IsValid()
        {
            if (FullPath == null)
                return false;
            if (IsFolder)
            {
                if (!Directory.Exists(FullPath))
                    return false;
            }
            else
            {
                if (!File.Exists(FullPath))
                    return false;
            }

            if (FileName == null)
                return false;

            return true;
        }

        public List<FunctionPathInfo> Paths = new List<FunctionPathInfo>();

        public void BuildMenus(ToolStripMenuItem ti, MouseEventHandler click_ev, EventHandler hoverEnter_ev, EventHandler hoverLeave_ev)
        {
            if (Paths.Count == 0)
            {
                if(Extension == ".gh")
                {
                    ToolStripItem item = ti.DropDownItems.Add(FileName);
                    item.MouseDown += click_ev;
                    item.MouseEnter += hoverEnter_ev;
                    item.MouseLeave += hoverLeave_ev;
                    item.Name = FullPath;
                }
            }
            else
            {
                ToolStripMenuItem item;

                if (isRoot)
                    item = ti;
                else
                {
                    item = new ToolStripMenuItem(FileName);
                    ti.DropDownItems.Add(item);
                }
                foreach (FunctionPathInfo p in Paths)
                {
                    p.BuildMenus(item, click_ev, hoverEnter_ev, hoverLeave_ev);
                }
            }
        }

        public void RemoveEmptyMenuItems(ToolStripMenuItem ti, MouseEventHandler click_ev, EventHandler hoverEnter_ev, EventHandler hoverLeave_ev)
        {
            List<int> indices = new List<int>();
            foreach(ToolStripMenuItem item in ti.DropDownItems)
            {
                if(item.DropDownItems.Count == 0 && Path.GetExtension(item.Name) == "")
                {
                    int index = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
                    item.MouseDown -= click_ev;
                    item.MouseEnter -= hoverEnter_ev;
                    item.MouseLeave -= hoverLeave_ev;
                    indices.Add(index);
                }
            }
            for(int i = 0; i < indices.Count; i++)
            {
                ti.DropDownItems.RemoveAt(indices[i] - i);
            }
        }
    }

    public class UriFunctionPathInfo
    {
        public UriFunctionPathInfo(string _endpoint, bool _isfolder)
        {
            EndPoint = _endpoint;
            IsFolder = _isfolder;
        }

        public string EndPoint { get; set; }
        public string FullPath { get; set; }
        public bool IsFolder { get; set; } = false;
        public bool isRoot { get; set; } = false;
        public string RootURL { get; set; }


        public List<UriFunctionPathInfo> Paths = new List<UriFunctionPathInfo>();

        public void BuildMenus(ToolStripMenuItem ti, MouseEventHandler click_ev)
        {
            if (Paths.Count == 0)
            {
                var ep = EndPoint;
                if (ep.StartsWith("/"))
                    ep = ep.Substring(1);
                
                ToolStripItem item = ti.DropDownItems.Add(ep);
                item.MouseDown += click_ev;
                item.Tag = FullPath;
            }
            else
            {
                ToolStripMenuItem item;

                if (isRoot)
                    item = ti;
                else
                {
                    var ep = EndPoint;
                    if (ep.StartsWith("/"))
                        ep = ep.Substring(1);

                    item = new ToolStripMenuItem(ep);
                    ti.DropDownItems.Add(item);
                }
                foreach (UriFunctionPathInfo p in Paths)
                {
                    p.BuildMenus(item, click_ev);
                }
            }
        }
    }

    public class FunctionMgr_ParamSchema
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Description { get; set; }
        public string ParamType { get; set; }
        public string ResultType { get; set; }
        public int AtLeast { get; set; }
        public int AtMost { get; set; }
    }
    public class FunctionMgr_Schema
    {
        public string Uri { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public FunctionMgr_ParamSchema[] Inputs { get; set; }
        public FunctionMgr_ParamSchema[] Outputs { get; set; }
        public string Icon { get; set; }
    }
}
