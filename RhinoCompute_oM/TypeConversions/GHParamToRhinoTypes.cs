using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public static partial class TypeConversions
    {
        public static Dictionary<Type, Type> GHParamToRhinoTypes { get; } = new Dictionary<Type, Type>()
        {
            { typeof(Param_Point), typeof(Point3d) },
            { typeof(Param_Vector), typeof(Vector3d) },
            { typeof(Param_Integer), typeof(int) },
            { typeof(Param_Number), typeof(double) },
            { typeof(Param_String), typeof(string) },
            { typeof(Param_Line), typeof(Line) },
            { typeof(Param_Circle), typeof(Circle) },
            { typeof(Param_Plane), typeof(Plane) },
            { typeof(Param_Rectangle), typeof(Rectangle3d) },
            { typeof(Param_Box), typeof(Box) },
            { typeof(Param_Surface), typeof(Surface) },
            { typeof(Param_Brep), typeof(Brep) },
            { typeof(GH_NumberSlider), typeof(double) },
            { typeof(Param_Boolean), typeof(bool) },
            { typeof(GH_BooleanToggle), typeof(bool) },
            { typeof(Param_Curve), null}, // Param_Curve may correspond to either Rhino.Geometry.Polyline or Rhino.Geometry.Curve
            { typeof(Param_GenericObject), typeof(object)}
        };
    }
}
