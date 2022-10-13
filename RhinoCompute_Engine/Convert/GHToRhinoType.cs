using System;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel.Types;
using log = BH.Engine.RemoteCompute.Log;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static Type GHToRhinoType(this Type t, bool enableWarnings = true)
        {
            if (!typeof(IGH_Goo).IsAssignableFrom(t) && enableWarnings)
            {
                log.RecordWarning($"Input type {t.FullName} is not a Grasshopper type (IGH_Goo).");
                return null;
            }

            Type equivalentRhinoType = null;
            if (TypeConversions.GHToRhinoTypes.TryGetValue(t, out equivalentRhinoType))
                return equivalentRhinoType;

            if (enableWarnings)
                log.RecordWarning($"No equivalent Rhino type found for type: {t.FullName}");

            return null;
        }
    }
}
