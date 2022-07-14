using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static Type ParamType(this IGH_Param param)
        {
            if (param == null)
                return null;

            Type t = param.GetType();

            var commonTypesInSources = param.Sources?.Select(s => s.GetType())?.Distinct();

            if (commonTypesInSources?.Count() == 1)
                return commonTypesInSources.FirstOrDefault();

            return null;
        }
    }
}
