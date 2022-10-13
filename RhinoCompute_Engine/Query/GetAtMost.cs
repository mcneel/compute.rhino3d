using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static int GetAtMost(this IGH_Param param)
        {
            IGH_ContextualParameter contextualParameter = param as IGH_ContextualParameter;

            if (contextualParameter != null)
                return contextualParameter.AtMost;

            if (contextualParameter is GH_NumberSlider paramSlider)
                return 1;

            return int.MaxValue;
        }
    }
}
