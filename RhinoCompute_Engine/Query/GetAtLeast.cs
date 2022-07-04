using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static int GetAtLeast(this InputGroup inputGroup)
        {
            IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;
            if (contextualParameter != null)
            {
                return contextualParameter.AtLeast;
            }
            return 1;
        }
    }
}
