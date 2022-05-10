using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using BH.oM.RemoteCompute.RhinoCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static string Description(this InputGroup inputgroup)
        {
            IGH_ContextualParameter contextualParameter = inputgroup.Param as IGH_ContextualParameter;
            if (contextualParameter != null)
                return contextualParameter.Prompt;

            return null;
        }
    }
}
