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

                IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    switch (inputGroup.Param.ParamTypeName())
                    {
                        case "Boolean":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    bool[] booleans = new bool[entree.Value.Count];
                                    for (int i = 0; i < booleans.Length; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        booleans[i] = JsonConvert.DeserializeObject<bool>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(booleans);
                                    break;
                                }
                            }
                            break;
                        case "Number":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    double[] doubles = new double[entree.Value.Count];
                                    for (int i = 0; i < doubles.Length; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        doubles[i] = JsonConvert.DeserializeObject<double>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(doubles);
                                    break;
                                }
                            }
                            break;
                        case "Integer":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    int[] integers = new int[entree.Value.Count];
                                    for (int i = 0; i < integers.Length; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        integers[i] = JsonConvert.DeserializeObject<int>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(integers);
                                    break;
                                }
                            }
                            break;
                        case "Point":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    Point3d[] points = new Point3d[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        points[i] = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(points);
                                    break;
                                }
                            }
                            break;
                        case "Line":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    Line[] lines = new Line[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        lines[i] = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(lines);
                                    break;
                                }
                            }
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
                    continue;
                }

                inputGroup.Param.VolatileData.Clear();
                inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet!

                if (inputGroup.Param is Param_Point)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Point3d rPt = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                            GH_Point data = new GH_Point(rPt);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Vector)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Vector3d rhVector = JsonConvert.DeserializeObject<Rhino.Geometry.Vector3d>(restobj.Data);
                            GH_Vector data = new GH_Vector(rhVector);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Integer)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            int rhinoInt = JsonConvert.DeserializeObject<int>(restobj.Data);
                            GH_Integer data = new GH_Integer(rhinoInt);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Number)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                            GH_Number data = new GH_Number(rhNumber);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_String)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            string rhString = restobj.Data;
                            GH_String data = new GH_String(rhString);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Line)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Line rhLine = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                            GH_Line data = new GH_Line(rhLine);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Curve)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
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
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Circle rhCircle = JsonConvert.DeserializeObject<Rhino.Geometry.Circle>(restobj.Data);
                            GH_Circle data = new GH_Circle(rhCircle);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Plane)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Plane rhPlane = JsonConvert.DeserializeObject<Rhino.Geometry.Plane>(restobj.Data);
                            GH_Plane data = new GH_Plane(rhPlane);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Rectangle)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Rectangle3d rhRectangle = JsonConvert.DeserializeObject<Rhino.Geometry.Rectangle3d>(restobj.Data);
                            GH_Rectangle data = new GH_Rectangle(rhRectangle);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Box)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Box rhBox = JsonConvert.DeserializeObject<Rhino.Geometry.Box>(restobj.Data);
                            GH_Box data = new GH_Box(rhBox);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Surface)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Surface rhSurface = JsonConvert.DeserializeObject<Rhino.Geometry.Surface>(restobj.Data);
                            GH_Surface data = new GH_Surface(rhSurface);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Brep)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Brep rhBrep = JsonConvert.DeserializeObject<Rhino.Geometry.Brep>(restobj.Data);
                            GH_Brep data = new GH_Brep(rhBrep);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Mesh)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Mesh rhMesh = JsonConvert.DeserializeObject<Rhino.Geometry.Mesh>(restobj.Data);
                            GH_Mesh data = new GH_Mesh(rhMesh);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is GH_NumberSlider)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                            GH_Number data = new GH_Number(rhNumber);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Boolean || inputGroup.Param is GH_BooleanToggle)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            bool boolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                            GH_Boolean data = new GH_Boolean(boolean);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is GH_Panel)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            string rhString = JsonConvert.DeserializeObject<string>(restobj.Data);
                            GH_String data = new GH_String(rhString);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }
            }

        }
    }
}
