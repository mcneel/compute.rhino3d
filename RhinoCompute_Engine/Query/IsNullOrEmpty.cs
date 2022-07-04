using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsNullOrEmpty(this ResthopperInput obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Script) && obj.Data.IsNullOrEmpty())
                return true;

            return false;
        }
    }
}
