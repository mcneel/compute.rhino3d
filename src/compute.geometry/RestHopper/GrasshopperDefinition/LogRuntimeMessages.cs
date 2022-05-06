using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RhinoCompute;
using Grasshopper.Kernel;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        private void LogRuntimeMessages(IEnumerable<IGH_ActiveObject> objects, ResthopperOutput schema)
        {
            foreach (var obj in objects)
                LogRuntimeMessages(obj, schema);
        }

        private void LogRuntimeMessages(IGH_ActiveObject obj, ResthopperOutput schema)
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
