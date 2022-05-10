using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static int GetAtMost(this InputGroup inputGroup)
        {
            IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;

            if (contextualParameter != null)
                return contextualParameter.AtMost;

            if (inputGroup.Param is GH_NumberSlider)
                return 1;

            return int.MaxValue;
        }
    }
}
