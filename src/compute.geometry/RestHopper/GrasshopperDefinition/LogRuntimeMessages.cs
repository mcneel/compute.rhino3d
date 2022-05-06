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
        private void LogRuntimeMessages(IEnumerable<IGH_ActiveObject> objects, FullRhinoComputeSchema schema)
        {
            foreach (var obj in objects)
                LogRuntimeMessages(obj, schema);
        }

        private void LogRuntimeMessages(IGH_ActiveObject obj, FullRhinoComputeSchema schema)
        {
            foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Error))
            {
                string errorMsg = $"{msg}: component \"{obj.Name}\" ({obj.InstanceGuid})";
                LogError(errorMsg);
                schema.Errors.Add(errorMsg);
                HasErrors = true;
            }

            if (Config.Debug)
            {
                foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Warning))
                {
                    string warningMsg = $"{msg}: component \"{obj.Name}\" ({obj.InstanceGuid})";
                    LogDebug(warningMsg);
                    schema.Warnings.Add(warningMsg);
                }
                foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Remark))
                {
                    LogDebug($"Remark in grasshopper component: \"{obj.Name}\" ({obj.InstanceGuid}): {msg}");
                }
            }
        }
    }
}
