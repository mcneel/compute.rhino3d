using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BH.oM.RemoteCompute.RhinoCompute;

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
