using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace compute.geometry
{
    static partial class GrasshopperDefinitionUtils
    {
        private static void AddInput(GrasshopperDefinition rc, IGH_Param param, string inputName)
        {
            if (rc.Inputs.ContainsKey(inputName))
            {
                string msg = "Multiple input parameters with the same name were detected. Parameter names must be unique.";
                rc.Remarks.Add(msg);
                Serilog.Log.Error(msg);
            }
            else
                rc.Inputs[inputName] = new InputGroup(param);
        }
    }
}
