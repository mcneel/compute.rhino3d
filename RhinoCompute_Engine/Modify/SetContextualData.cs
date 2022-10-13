using System;
using System.Collections.Generic;
using BH.oM.Computing;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Modify
    {
        public static bool SetContextualData(this IGH_ContextualParameter contextualParameter, string paramTypeName, GrasshopperDataTree<ResthopperObject> data)
        {
            if (contextualParameter == null || string.IsNullOrEmpty(paramTypeName))
                return false;

            switch (paramTypeName)
            {
                case "Boolean":
                    return AssignContextualDataGeneric<bool>(contextualParameter, data);
                case "Number":
                    return AssignContextualDataGeneric<double>(contextualParameter, data);
                case "Integer":
                    return AssignContextualDataGeneric<int>(contextualParameter, data);
                case "Point":
                    return AssignContextualDataGeneric<Point3d>(contextualParameter, data);
                case "Line":
                    return AssignContextualDataGeneric<Line>(contextualParameter, data);
                case "Text":
                    foreach (KeyValuePair<string, List<ResthopperObject>> entry in data)
                    {
                        string[] strings = new string[entry.Value.Count];
                        for (int i = 0; i < entry.Value.Count; i++)
                        {
                            ResthopperObject restobj = entry.Value[i];
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
                    return true;
                case "Geometry":
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
                    return true;
            }

            return false;
        }

        public static bool AssignContextualDataGeneric<T>(IGH_ContextualParameter contextualParameter, GrasshopperDataTree<ResthopperObject> data)
        {
            if (contextualParameter == null)
                return false;

            try
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

                return true;
            }
            catch (Exception e)
            {
                Log.RecordError(e.Message);
                return false;
            }
        }
    }
}
