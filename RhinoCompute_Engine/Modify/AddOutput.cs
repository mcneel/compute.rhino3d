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
                Log.RecordError($"Multiple outputs with the same name `{name}` were detected.");
            else
                rc.Outputs[name] = new Output(name, param, param.Description());
        }
    }
}
