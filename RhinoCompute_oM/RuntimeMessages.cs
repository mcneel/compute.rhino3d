
using System.Collections.Generic;
using BH.oM.Base;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public class RuntimeMessages : IObject
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Remarks { get; set; } = new List<string>();
    }
}
