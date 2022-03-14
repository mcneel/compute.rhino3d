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

        public void BuildMenus(ToolStripMenuItem ti, MouseEventHandler ev)
        {
            if (Paths.Count == 0)
            {
                ToolStripItem item = ti.DropDownItems.Add(FileName);
                item.MouseDown += ev;
                item.Name = FullPath;
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
                    p.BuildMenus(item, ev);
                }
            }
        }
    }
    
}
