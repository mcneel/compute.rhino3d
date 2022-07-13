using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static string Description(this IGH_ContextualParameter contextualParameter)
        {
            if (contextualParameter != null)
                return contextualParameter.Prompt;

            return null;
        }
    }
}
