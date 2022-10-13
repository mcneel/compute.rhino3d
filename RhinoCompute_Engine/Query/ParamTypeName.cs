using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static string ParamTypeName(this IGH_Param param)
        {
            if (param == null)
                return null;

            Type t = param.GetType();
            if (t.Name.Equals("GetGeometryParameter"))
                return "Geometry"; // Needed workaround to avoid bug in GH

            return param.TypeName;
        }

        public static string ParamTypeNameIncludingSources(this IGH_Param param)
        {
            if (param == null)
                return null;

            var commonTypesInSources = param.Sources?.Select(s => s.GetType())?.Distinct();

            if (commonTypesInSources?.Count() == 1 && commonTypesInSources.FirstOrDefault().AssemblyQualifiedName.StartsWith("BH.UI.Grasshopper"))
            {
                List<Type> sourceTypes = new List<Type>();
                foreach (var source in param.Sources)
                {
                    try
                    {
                        sourceTypes.Add((source as dynamic).ObjectType);
                    }
                    catch { }
                }

                sourceTypes = sourceTypes.Distinct().ToList();

                if (sourceTypes.Count == 1)
                    if (param.Sources.Count == 1)
                        return sourceTypes.First().FullName;
                    else
                    {
                        Type listType = typeof(List<>).MakeGenericType(sourceTypes.First());
                        return listType.FullName;
                    }
            }

            return ParamTypeName(param);
        }

        public static string ParamTypeNameIncludingRecipients(this IGH_Param param)
        {
            if (param == null)
                return null;

            var commonTypesInRecipients = param.Recipients?.Select(s => s.GetType())?.Distinct();

            if (commonTypesInRecipients?.Count() == 1 && commonTypesInRecipients.FirstOrDefault().AssemblyQualifiedName.StartsWith("BH.UI.Grasshopper"))
            {
                var recipient = param.Recipients.FirstOrDefault();

                try
                {
                    return (recipient as dynamic).ObjectType.FullName;
                }
                catch { }
            }

            return ParamTypeName(param);
        }
    }
}
