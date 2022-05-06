using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public bool AssignContextualData(IGH_Param ighParam, GrasshopperDataTree<ResthopperObject> tree)
        {
            IGH_ContextualParameter contextualParameter = ighParam as IGH_ContextualParameter;
            if (ighParam == null)
                return false;

            switch (ighParam.ParamTypeName())
            {
                case "Boolean":
                    AssignContextualData<bool>(contextualParameter, tree);
                    break;
                case "Number":
                    AssignContextualData<double>(contextualParameter, tree);
                    break;
                case "Integer":
                    AssignContextualData<int>(contextualParameter, tree);
                    break;
                case "Point":
                    AssignContextualData<Point3d>(contextualParameter, tree);
                    break;
                case "Line":
                    AssignContextualData<Line>(contextualParameter, tree);
                    break;
                case "Text":
                    {
                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
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
                                catch (Exception)
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
                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
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

        public void AssignContextualData<T>(IGH_ContextualParameter contextualParameter, GrasshopperDataTree<ResthopperObject> tree)
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
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
