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
        private static void AddInput(GrasshopperDefinition rc, IGH_Param param, string name)
        {
            if (rc._input.ContainsKey(name))
            {
                string msg = "Multiple input parameters with the same name were detected. Parameter names must be unique.";
                rc.HasErrors = true;
                rc.ErrorMessages.Add(msg);
                LogError(msg);
            }
            else
                rc._input[name] = new InputGroup(param);
        }
    }
}
