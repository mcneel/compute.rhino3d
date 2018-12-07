using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using System.IO;
using Resthopper.IO;

namespace Resthopper.GH
{
    public class SaveTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SaveTree class.
        /// </summary>
        public SaveTree()
          : base("Save Tree", "Save Tree",
              "Description",
              "Resthopper", "Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("tree", "tree", "tree to serialize", GH_ParamAccess.tree);
            pManager.AddTextParameter("path", "path", "path to save the file", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("tree", "tree", "tree to path through", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Grasshopper.Kernel.Data.GH_Structure<IGH_Goo> tree = new Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>();
            string path = string.Empty;

            DA.GetDataTree(0, out tree);
            DA.GetData(1, ref path);
            Resthopper.IO.DataTree<ResthopperObject> OutputTree = new Resthopper.IO.DataTree<ResthopperObject>();

            var volatileData = this.Params.Input[0].Sources[0].VolatileData;
            for (int p = 0; p < volatileData.PathCount; p++)
            {
                List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                foreach (var goo in volatileData.get_Branch(p))
                {
                    if (goo == null) continue;
                    else if (goo.GetType() == typeof(GH_Boolean))
                    {
                        GH_Boolean ghValue = goo as GH_Boolean;
                        bool rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<bool>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Point))
                    {
                        GH_Point ghValue = goo as GH_Point;
                        Point3d rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Point3d>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Vector))
                    {
                        GH_Vector ghValue = goo as GH_Vector;
                        Vector3d rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Vector3d>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Integer))
                    {
                        GH_Integer ghValue = goo as GH_Integer;
                        int rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<int>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Number))
                    {
                        GH_Number ghValue = goo as GH_Number;
                        double rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<double>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_String))
                    {
                        GH_String ghValue = goo as GH_String;
                        string rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<string>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Line))
                    {
                        GH_Line ghValue = goo as GH_Line;
                        Line rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Line>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Curve))
                    {
                        GH_Curve ghValue = goo as GH_Curve;
                        Curve rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Curve>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Circle))
                    {
                        GH_Circle ghValue = goo as GH_Circle;
                        Circle rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Circle>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Plane))
                    {
                        GH_Plane ghValue = goo as GH_Plane;
                        Plane rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Plane>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Rectangle))
                    {
                        GH_Rectangle ghValue = goo as GH_Rectangle;
                        Rectangle3d rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Rectangle3d>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Box))
                    {
                        GH_Box ghValue = goo as GH_Box;
                        Box rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Box>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Surface))
                    {
                        GH_Surface ghValue = goo as GH_Surface;
                        Brep rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Brep))
                    {
                        GH_Brep ghValue = goo as GH_Brep;
                        Brep rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                    }
                    else if (goo.GetType() == typeof(GH_Mesh))
                    {
                        GH_Mesh ghValue = goo as GH_Mesh;
                        Mesh rhValue = ghValue.Value;
                        ResthopperObjectList.Add(GetResthopperObject<Mesh>(rhValue));
                    }
                }

                GhPath ghpath = new GhPath(new int[] { p });
                OutputTree.Add(ghpath.ToString(), ResthopperObjectList);
            }






            string serialized = JsonConvert.SerializeObject(OutputTree);
            using(StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(serialized);
            }
            DA.SetDataTree(0, tree);
        }

        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v);
            return rhObj;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8fda0216-6111-49b6-93f5-e7c82b2ce1c1"); }
        }
    }
}