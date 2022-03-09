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
        private GrasshopperDefinition(GH_Document gh_document, string icon)
        {
            GH_Document = gh_document;
            _iconString = icon;
            FileRuntimeCacheSerialNumber = _watchedFileRuntimeSerialNumber;
        }

        public GH_Document GH_Document { get; }
        public bool InDataCache { get; set; }
        public bool HasErrors { get; private set; } // default: false
        public bool IsLocalFileDefinition { get; set; } // default: false
        public uint FileRuntimeCacheSerialNumber { get; private set; }
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
