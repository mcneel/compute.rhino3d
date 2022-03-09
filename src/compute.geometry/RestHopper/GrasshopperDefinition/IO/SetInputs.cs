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

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public void AssignContextualData<T>(IGH_ContextualParameter contextualParameter, DataTree<ResthopperObject> tree)
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

        public void AssignVolatileData<RType, GHType>(IGH_Param gH_Param, DataTree<ResthopperObject> tree, Func<RType, GHType> create)
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entry.Key));
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    ResthopperObject restobj = entry.Value[i];
                    RType rPt = JsonConvert.DeserializeObject<RType>(restobj.Data);
                    GHType data = create(rPt);
                    gH_Param.AddVolatileData(path, i, data);
                }
            }
        }

        public bool AssignContextualData(IGH_Param ighParam, DataTree<ResthopperObject> tree)
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

        public void SetInputs(List<DataTree<ResthopperObject>> values)
        {
            InputGroup inputGroup = null;

            foreach (DataTree<ResthopperObject> tree in values)
            {
                if (!_input.TryGetValue(tree.ParamName, out inputGroup))
                    continue;

                if (inputGroup.IsAlreadySet(tree))
                {
                    LogDebug("Skipping input tree... same input");
                    continue;
                }

                inputGroup.StoreTree(tree);

                if (AssignContextualData(inputGroup.Param, tree))
                    continue;

                inputGroup.Param.VolatileData.Clear();
                inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet!

                if (inputGroup.Param is Param_Point)
                {
                    AssignVolatileData<Point3d, GH_Point>(inputGroup.Param, tree, c => new GH_Point(c));
                    continue;
                }

                if (inputGroup.Param is Param_Vector)
                {
                    AssignVolatileData<Vector3d, GH_Vector>(inputGroup.Param, tree, c => new GH_Vector(c));
                    continue;
                }

                if (inputGroup.Param is Param_Integer)
                {
                    AssignVolatileData<int, GH_Integer>(inputGroup.Param, tree, c => new GH_Integer(c));
                    continue;
                }

                if (inputGroup.Param is Param_Number)
                {
                    AssignVolatileData<double, GH_Number>(inputGroup.Param, tree, c => new GH_Number(c));
                    continue;
                }

                if (inputGroup.Param is Param_String || inputGroup.Param is GH_Panel)
                {
                    AssignVolatileData<string, GH_String>(inputGroup.Param, tree, c => new GH_String(c));
                    continue;
                }

                if (inputGroup.Param is Param_Line)
                {
                    AssignVolatileData<Line, GH_Line>(inputGroup.Param, tree, c => new GH_Line(c));
                    continue;
                }

                if (inputGroup.Param is Param_Curve)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entry.Key));
                        for (int i = 0; i < entry.Value.Count; i++)
                        {
                            ResthopperObject restobj = entry.Value[i];
                            GH_Curve ghCurve;
                            try
                            {
                                Rhino.Geometry.Polyline data = JsonConvert.DeserializeObject<Rhino.Geometry.Polyline>(restobj.Data);
                                Rhino.Geometry.Curve c = new Rhino.Geometry.PolylineCurve(data);
                                ghCurve = new GH_Curve(c);
                            }
                            catch
                            {
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                                var c = (Rhino.Geometry.Curve)Rhino.Runtime.CommonObject.FromJSON(dict);
                                ghCurve = new GH_Curve(c);
                            }
                            inputGroup.Param.AddVolatileData(path, i, ghCurve);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Circle)
                {
                    AssignVolatileData<Circle, GH_Circle>(inputGroup.Param, tree, c => new GH_Circle(c));
                    continue;
                }

                if (inputGroup.Param is Param_Plane)
                {
                    AssignVolatileData<Plane, GH_Plane>(inputGroup.Param, tree, c => new GH_Plane(c));
                    continue;
                }

                if (inputGroup.Param is Param_Rectangle)
                {
                    AssignVolatileData<Rectangle3d, GH_Rectangle>(inputGroup.Param, tree, c => new GH_Rectangle(c));
                    continue;
                }

                if (inputGroup.Param is Param_Box)
                {
                    AssignVolatileData<Box, GH_Box>(inputGroup.Param, tree, c => new GH_Box(c));

                    continue;
                }

                if (inputGroup.Param is Param_Surface)
                {
                    AssignVolatileData<Surface, GH_Surface>(inputGroup.Param, tree, c => new GH_Surface(c));
                    continue;
                }

                if (inputGroup.Param is Param_Brep)
                {
                    AssignVolatileData<Brep, GH_Brep>(inputGroup.Param, tree, c => new GH_Brep(c));
                    continue;
                }

                if (inputGroup.Param is Param_Mesh)
                {
                    AssignVolatileData<Mesh, GH_Mesh>(inputGroup.Param, tree, c => new GH_Mesh(c));
                    continue;
                }

                if (inputGroup.Param is GH_NumberSlider)
                {
                    AssignVolatileData<double, GH_Number>(inputGroup.Param, tree, c => new GH_Number(c));
                    continue;
                }

                if (inputGroup.Param is Param_Boolean || inputGroup.Param is GH_BooleanToggle)
                {
                    AssignVolatileData<bool, GH_Boolean>(inputGroup.Param, tree, c => new GH_Boolean(c));
                    continue;
                }
            }
        }
    }
}
