using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AddOutput(this GrasshopperDefinition rc, IGH_Param param, string name)
        {
            if (param == null || string.IsNullOrWhiteSpace(name))
                return;

            if (rc.Outputs.ContainsKey(name))
                rc.Errors.Add("Multiple output parameters with the same name were detected. Parameter names must be unique.");
            else
                rc.Outputs[name] = param;
        }
    }
}
