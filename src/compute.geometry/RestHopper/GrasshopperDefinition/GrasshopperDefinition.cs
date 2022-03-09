using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;
using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        private GrasshopperDefinition(GH_Document gh_document)
        {
            GH_Document = gh_document;
            FileRuntimeCacheSerialNumber = _watchedFileRuntimeSerialNumber;
        }

        private GrasshopperDefinition(GH_Document gh_document, string icon) : this(gh_document)
        {
            _iconString = icon;
        }

        public GH_Document GH_Document { get; }

        public bool FoundInDataCache { get; set; }
        public bool HasErrors { get; private set; } = false;
        public bool IsLocalFileDefinition { get; set; } = false;

        public uint FileRuntimeCacheSerialNumber { get; private set; } = _watchedFileRuntimeSerialNumber;
        public static uint WatchedFileRuntimeSerialNumber { get { return _watchedFileRuntimeSerialNumber; } }

        static Dictionary<string, FileSystemWatcher> _filewatchers;
        static HashSet<string> _watchedFiles = new HashSet<string>();
        static uint _watchedFileRuntimeSerialNumber = 1;

        public string CacheKey { get; set; }
        string _iconString;
        GH_Component _singularComponent;
        Dictionary<string, InputGroup> _input = new Dictionary<string, InputGroup>();
        Dictionary<string, IGH_Param> _output = new Dictionary<string, IGH_Param>();
        public List<string> ErrorMessages = new List<string>();
        static void LogDebug(string message) { Serilog.Log.Debug(message); }
        static void LogError(string message) { Serilog.Log.Error(message); }
    }
}
