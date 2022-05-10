using System.Collections.Generic;
using BH.oM.RemoteCompute;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static bool AssignContextualData(this IGH_Param ighParam, GrasshopperDataTree<ResthopperObject> data)
        {
            IGH_ContextualParameter contextualParameter = ighParam as IGH_ContextualParameter;
            if (ighParam == null)
                return false;

            switch (ighParam.ParamTypeName())
            {
                case "Boolean":
                    AssignContextualData<bool>(contextualParameter, data);
                    break;
                case "Number":
                    AssignContextualData<double>(contextualParameter, data);
                    break;
                case "Integer":
                    AssignContextualData<int>(contextualParameter, data);
                    break;
                case "Point":
                    AssignContextualData<Point3d>(contextualParameter, data);
                    break;
                case "Line":
                    AssignContextualData<Line>(contextualParameter, data);
                    break;
                case "Text":
                    {
                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in data)
                        {
                            string[] strings = new string[entree.Value.Count];
                            for (int i = 0; i < entree.Value.Count; i++)
                            {
                                ResthopperObject restobj = entree.Value[i];
                                // 2 July 2021 S. Baer (Github issue #394)
                                // This is pretty hacky and I wish I understood json.net a bit more
                                // to figure out why it is throwing exceptions in certain cases.
                                // I'm hoping to support both embedded json inside of other json as
                                // well as plain strings.
                                try
                                {
                                    // Use JsonConvert to properly unescape the string
                                    strings[i] = JsonConvert.DeserializeObject<string>(restobj.Data);
                                }
                                catch 
                                {
                                    strings[i] = System.Text.RegularExpressions.Regex.Unescape(restobj.Data);
                                }
                            }
                            contextualParameter.AssignContextualData(strings);
                            break;
                        }
                    }
                    break;
                case "Geometry":
                    {
                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in data)
                        {
                            GeometryBase[] geometries = new GeometryBase[entree.Value.Count];
                            for (int i = 0; i < entree.Value.Count; i++)
                            {
                                ResthopperObject restobj = entree.Value[i];
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                                geometries[i] = Rhino.Runtime.CommonObject.FromJSON(dict) as GeometryBase;
                            }
                            contextualParameter.AssignContextualData(geometries);
                            break;
                        }
                    }
                    break;
            }

            return true;
        }

        public static void AssignContextualData<T>(IGH_ContextualParameter contextualParameter, GrasshopperDataTree<ResthopperObject> data)
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entry in data)
            {
                T[] values = new T[entry.Value.Count];
                for (int i = 0; i < values.Length; i++)
                {
                    ResthopperObject restobj = entry.Value[i];
                    values[i] = JsonConvert.DeserializeObject<T>(restobj.Data);
                }

                contextualParameter.AssignContextualData(values);
            }
        }
    }
}
