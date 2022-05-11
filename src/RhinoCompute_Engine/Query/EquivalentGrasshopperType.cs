using System;
using BH.oM.RemoteCompute.RhinoCompute;
using log = BH.Engine.RemoteCompute.Log;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static Type EquivalentGrasshopperType(this Type t, bool warningIfNotFound = true)
        {
            Type equivalentGrasshopperType = null;
            if (GrasshopperRhinoTypes.RhinoToGrasshopperTypes.TryGetValue(t, out equivalentGrasshopperType))
                return equivalentGrasshopperType;

            if (warningIfNotFound)
                log.RecordWarning($"No equivalent Grasshopper type found for type: {t.FullName}");

            return null;
        }
    }
}
