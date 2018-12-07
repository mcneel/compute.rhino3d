using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using Newtonsoft.Json;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Resthopper.IO;

namespace Resthopper.GH
{
    public class LoadTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LoadTree class.
        /// </summary>
        public LoadTree()
          : base("Load Tree", "Load Tree",
              "Description",
              "Resthopper", "Debug")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("path", "path", "path to saved tree", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("tree", "tree", "loaded tree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = string.Empty;
            DA.GetData(0, ref path);
            Resthopper.IO.DataTree<ResthopperObject> tree;
            using (StreamReader reader = new StreamReader(path))
            {
                string serialized = reader.ReadToEnd();
                tree = JsonConvert.DeserializeObject<Resthopper.IO.DataTree<ResthopperObject>> (serialized);
            }

            Grasshopper.Kernel.Data.GH_Structure<IGH_Goo> ghTree = new Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>();
            foreach (KeyValuePair<string, List<ResthopperObject>> pair in tree)
            {
                List<IGH_Goo> pLines = new List<IGH_Goo>();
                Grasshopper.Kernel.Data.GH_Path ghPath = new Grasshopper.Kernel.Data.GH_Path(GhPath.FromString(pair.Key));
                foreach (ResthopperObject ro in pair.Value)
                {

                    try { ghTree.Append(new GH_Curve(JsonConvert.DeserializeObject<Curve>(ro.Data)), ghPath); }
                    catch
                    {
                        try { ghTree.Append(new GH_Point(JsonConvert.DeserializeObject<Point3d>(ro.Data)), ghPath); }
                        catch
                        {
                            ghTree.Append(new GH_Boolean(JsonConvert.DeserializeObject<bool>(ro.Data)), ghPath);
                        }
                    }
                }
                
            }
                    DA.SetDataTree(0, ghTree);
                 
            
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
            get { return new Guid("f9c34ede-a64f-43ef-806d-314443637b54"); }
        }
    }
}