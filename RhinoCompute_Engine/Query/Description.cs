using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using System.Linq;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static string Description(this IRemoteIO io)
        {
            return io.Description;
        }

        public static string Description(this IGH_ContextualParameter contextualParameter)
        {
            if (contextualParameter == null)
                return null;

            IGH_Param param = contextualParameter as IGH_Param;

            if (param != null)
                return param.Description;

            return contextualParameter.Prompt;
        }

        public static string Description(this IGH_Param param, bool isDescriptionParam = false)
        {
            if (isDescriptionParam)
                return param.VolatileData.AllData(true).FirstOrDefault().ToString();

            if (param != null)
                return param.Description;

            return null;
        }

        public static string Description(this IGH_DocumentObject docObj)
        {
            if (docObj.IsRemoteInput())
            {
                IGH_Param descriptionParameter = (docObj as GH_Component).Params.Input.Where(p => p.Description.ToLower().Contains("description")).FirstOrDefault();
                if (descriptionParameter != null)
                    return (descriptionParameter as dynamic).PersistentData.ToString();
            }

            IGH_ContextualParameter contextualParameter = docObj as IGH_ContextualParameter;
            if (contextualParameter != null)
                return contextualParameter.Description();

            return null;
        }
    }
}
