
using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using Grasshopper.Kernel;

namespace compute.geometry
{
    partial class GrasshopperDefinitionUtils
    {
        private static void AddOutput(GrasshopperDefinition rc, IGH_Param param, string name)
        {
            if (rc.Outputs.ContainsKey(name))
            {
                string msg = "Multiple output parameters with the same name were detected. Parameter names must be unique.";
                rc.Remarks.Add(msg);
                Serilog.Log.Error(msg);
            }
            else
                rc.Outputs[name] = param;
        }
    }
}
