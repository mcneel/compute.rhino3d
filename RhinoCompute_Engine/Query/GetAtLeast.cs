using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static int GetAtLeast(this IGH_Param param)
        {
            IGH_ContextualParameter contextualParameter = param as IGH_ContextualParameter;

            if (contextualParameter != null)
                return contextualParameter.AtLeast;

            if (contextualParameter is GH_NumberSlider paramSlider)
                return 0;

            return 1;
        }
    }
}
