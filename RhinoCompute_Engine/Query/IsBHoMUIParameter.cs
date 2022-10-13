using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsBHoMUIParameter(this IGH_Param obj)
        {
            return obj.GetType().FullName.StartsWith("BH.UI");
        }
    }
}
