using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public static partial class TypeConversions
    {
        public static Dictionary<Type, Type> GHToRhinoTypes { get; } = new Dictionary<Type, Type>()
        {
            { typeof(GH_Boolean), typeof(bool) },
            { typeof(GH_Point), typeof(Point3d) },
            { typeof(GH_Vector), typeof(Vector3d) },
            { typeof(GH_Integer), typeof(int) },
            { typeof(GH_Number), typeof(double) },
            { typeof(GH_String), typeof(string) },
            { typeof(GH_SubD), typeof(SubD) },
            { typeof(GH_Line), typeof(Line) },
            { typeof(GH_Curve), typeof(Curve) },
            { typeof(GH_Circle), typeof(Circle) },
            { typeof(GH_Plane), typeof(Plane) },
            { typeof(GH_Rectangle), typeof(Rectangle3d) },
            { typeof(GH_Box), typeof(Box) },
            { typeof(GH_Surface), typeof(Brep) },
            { typeof(GH_Brep), typeof(Brep) },
            { typeof(GH_Mesh), typeof(Mesh) }
        };

        public static Dictionary<Type, Type> RhinoToGHTypes { get; } = new Dictionary<Type, Type>()
        {
            { typeof(bool), typeof(GH_Boolean) },
            { typeof(Point3d), typeof(GH_Point) },
            { typeof(Vector3d), typeof(GH_Vector) },
            { typeof(int) , typeof(GH_Integer)},
            { typeof(double), typeof(GH_Number) },
            { typeof(string), typeof(GH_String) },
            { typeof(SubD), typeof(GH_SubD) },
            { typeof(Line), typeof(GH_Line) },
            { typeof(Curve), typeof(GH_Curve) },
            { typeof(Circle), typeof(GH_Circle) },
            { typeof(Plane), typeof(GH_Plane)  },
            { typeof(Rectangle3d), typeof(GH_Rectangle) },
            { typeof(Box), typeof(GH_Box) },
            //{ typeof(Brep), typeof(GH_Surface) },
            { typeof(Brep), typeof(GH_Brep)},
            { typeof(Mesh), typeof(GH_Mesh) }
        };
    }
}
