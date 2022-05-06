using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using log = BH.Engine.RemoteCompute.Log;

namespace BH.Engine.RhinoCompute
{
    public static partial class Query
    {
        public static Type EquivalentRhinoType(this Type t, bool warningIfNotFound = true)
        {
            Type equivalentRhinoType = null;
            if (GrasshopperRhinoTypes.GrasshopperToRhinoTypes.TryGetValue(t, out equivalentRhinoType))
                return equivalentRhinoType;

            if (warningIfNotFound)
                log.RecordWarning($"No equivalent Rhino type found for type: {t.FullName}");

            return null;
        }
    }
}
