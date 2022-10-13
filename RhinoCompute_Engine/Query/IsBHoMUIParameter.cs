using System.Collections.Generic;
using System.Linq;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsBHoMUIParameter(this IGH_Param obj)
        {
            return obj.GetType().FullName.StartsWith("BH.UI");
        }
    }
}
