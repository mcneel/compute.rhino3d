using System;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static class Query
    {
        static string ParamTypeName(IGH_Param param)
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
