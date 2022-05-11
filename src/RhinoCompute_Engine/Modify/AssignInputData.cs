using System;
using System.Collections.Generic;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AssignInputData(this GrasshopperDefinition rc, List<GrasshopperDataTree<ResthopperObject>> inputsListTrees)
        {
            foreach (GrasshopperDataTree<ResthopperObject> tree in inputsListTrees)
            {
                // Make sure the input has been created before populating it with data.
                // This is done via AddInput().
                InputGroup inputGroup = null;
                if (!rc.Inputs.TryGetValue(tree.ParamName, out inputGroup))
                    continue;

                if (inputGroup.IsAlreadySet(tree))
                    continue;

                inputGroup.DataTree = tree;

                if (AssignContextualData(inputGroup.Param, tree))
                    continue;

                inputGroup.Param.VolatileData.Clear();
                inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet!

                if (inputGroup.Param is Param_Point)
                {
                    AssignVolatileData<Point3d, GH_Point>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Vector)
                {
                    AssignVolatileData<Vector3d, GH_Vector>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Integer)
                {
                    AssignVolatileData<int, GH_Integer>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Number)
                {
                    AssignVolatileData<double, GH_Number>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_String || inputGroup.Param is GH_Panel)
                {
                    AssignVolatileData<string, GH_String>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Line)
                {
                    AssignVolatileData<Line, GH_Line>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Curve)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
                    {
                        GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
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
                    AssignVolatileData<Circle, GH_Circle>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Plane)
                {
                    AssignVolatileData<Plane, GH_Plane>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Rectangle)
                {
                    AssignVolatileData<Rectangle3d, GH_Rectangle>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Box)
                {
                    AssignVolatileData<Box, GH_Box>(inputGroup.Param, tree);

                    continue;
                }

                if (inputGroup.Param is Param_Surface)
                {
                    AssignVolatileData<Surface, GH_Surface>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Brep)
                {
                    AssignVolatileData<Brep, GH_Brep>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Mesh)
                {
                    AssignVolatileData<Mesh, GH_Mesh>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is GH_NumberSlider)
                {
                    AssignVolatileData<double, GH_Number>(inputGroup.Param, tree);
                    continue;
                }

                if (inputGroup.Param is Param_Boolean || inputGroup.Param is GH_BooleanToggle)
                {
                    AssignVolatileData<bool, GH_Boolean>(inputGroup.Param, tree);
                    continue;
                }
            }
        }

        private static void AssignVolatileData<RType, GHType>(this IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree) where GHType : class
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entry in dataTree)
            {
                GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    ResthopperObject restobj = entry.Value[i];
                    RType rhinoData = JsonConvert.DeserializeObject<RType>(restobj.Data);
                    GHType grasshopperData = Activator.CreateInstance(typeof(GHType), new object[] { rhinoData }) as GHType; // new GHType(rhinoData);
                    gH_Param.AddVolatileData(path, i, grasshopperData);
                }
            }
        }
    }
}
