using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Modify
    {
        public static void AddInput(this GrasshopperDefinition rc, IGH_Param param, string inputName, string description = null)
        {
            if (param == null || string.IsNullOrWhiteSpace(inputName))
                return;

            if (rc.Inputs.ContainsKey(inputName))
                Log.RecordWarning($"Multiple inputs found under the name {inputName}. Considering only the first match.");
            else
                rc.Inputs[inputName] = new Input(inputName, param, description ?? param.Description());
        }
    }
}
