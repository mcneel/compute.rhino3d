using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;
using Rhino.Geometry;
using Grasshopper.Kernel;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition : IGrasshopperDefinition
    {
        private GrasshopperDefinition(GH_Document gh_document)
        {
            GH_Document = gh_document;
        }

        public Dictionary<string, InputGroup> Inputs { get; set; } = new Dictionary<string, InputGroup>();
        public Dictionary<string, IGH_Param> Outputs { get; set; } = new Dictionary<string, IGH_Param>();

        public GH_Document GH_Document { get; }
        public GH_Component SingularComponent { get; set; } = null;

        static Dictionary<string, FileSystemWatcher> _filewatchers;
        static HashSet<string> _watchedFiles = new HashSet<string>();
        static uint _watchedFileRuntimeSerialNumber = 1;
        public uint FileRuntimeCacheSerialNumber { get; private set; } = _watchedFileRuntimeSerialNumber;
        public static uint WatchedFileRuntimeSerialNumber { get { return _watchedFileRuntimeSerialNumber; } }

        public string CacheKey { get; set; }
        public bool FoundInDataCache { get; set; }
        public bool IsLocalFileDefinition { get; set; } = false;

        public bool HasErrors { get; private set; } = false;
        public List<string> ErrorMessages { get; set; } = new List<string>();
        static void LogDebug(string message) { Serilog.Log.Debug(message); }
        static void LogError(string message) { Serilog.Log.Error(message); }
    }
}
