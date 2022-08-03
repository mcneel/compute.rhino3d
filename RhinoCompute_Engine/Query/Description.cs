using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
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

            if (param is GH_Panel panel && !string.IsNullOrWhiteSpace(panel.NickName))
                return panel.NickName;

            if (param != null)
                return param.Description;

            return null;
        }

        public static string Description(this IGH_DocumentObject docObj)
        {
            if (docObj.IsRemoteInput() || docObj.IsRemoteOutput())
            {
                IGH_Param descriptionParameter = (docObj as GH_Component).Params.Input.Where(p => p.Name.ToLower().Contains("description")).FirstOrDefault();

                if (descriptionParameter == null)
                    return null;

                string description = descriptionParameter.PersistentData()?.FirstOrDefault()?.ToString();
                if (description != null)
                    return description;

                description = descriptionParameter.VolatileData()?.FirstOrDefault()?.ToString();
                if (description != null)
                    return description;

                var sources = descriptionParameter.Sources.FirstOrDefault();
                description = sources.PersistentData()?.FirstOrDefault()?.ToString();
                if (description != null)
                    return description;

                description = sources.VolatileData()?.FirstOrDefault()?.ToString();
                if (description != null)
                    return description;

                var panel = sources as Grasshopper.Kernel.Special.GH_Panel;
                if (panel != null)
                    return panel.UserText;
            }

            IGH_ContextualParameter contextualParameter = docObj as IGH_ContextualParameter;
            if (contextualParameter != null)
                return contextualParameter.Description();

            return null;
        }
    }
}
