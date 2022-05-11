using System;
using BH.oM.RemoteCompute.RhinoCompute;
using log = BH.Engine.RemoteCompute.Log;

namespace BH.Engine.RemoteCompute.RhinoCompute
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
