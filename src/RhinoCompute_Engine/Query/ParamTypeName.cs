using System;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static string ParamTypeName(this IGH_Param param)
        {
            Type t = param.GetType();
            // TODO: Figure out why the GetGeometryParameter throws exceptions when calling TypeName
            if (t.Name.Equals("GetGeometryParameter"))
            {
                return "Geometry";
            }
            return param.TypeName;
        }
    }
}
