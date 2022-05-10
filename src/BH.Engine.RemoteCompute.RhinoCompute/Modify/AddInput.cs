using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AddInput(this GrasshopperDefinition rc, IGH_Param param, string inputName)
        {
            if (param == null || string.IsNullOrWhiteSpace(inputName))
                return;

            if (rc.Inputs.ContainsKey(inputName))
                rc.Warnings.Add($"Multiple inputs found under the name {inputName}. Considering only the first match.");
            else
                rc.Inputs[inputName] = new InputGroup(param);
        }
    }
}
