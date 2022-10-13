using System;
using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel.Types;
using log = BH.Engine.Computing.Log;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Convert
    {
        public static Type RhinoToGHType(this Type t, bool enableWarnings = true)
        {
            if (t == null)
                return typeof(GH_ObjectWrapper);

            Type equivalentGrasshopperType = null;
            if (TypeConversions.RhinoToGHTypes.TryGetValue(t, out equivalentGrasshopperType))
                return equivalentGrasshopperType;

            if (enableWarnings)
                log.RecordWarning($"No equivalent Grasshopper type found for type: {t.FullName}");

            return null;
        }
    }
}
