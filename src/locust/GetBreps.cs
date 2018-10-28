using System;
using System.Collections.Generic;
using compute.geometry;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using Grasshopper.Kernel.Types;

namespace locust
{
    public class GetBreps : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GetBreps()
          : base("breps", "Get Breps",
              "Get Brep Geometry from compute.rhino3d",
              "Locust", "Get")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("serialized", "serialized", "response from the server", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "Breps", "Deserialized Breps", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string input = string.Empty;
            DA.GetData<string>(0, ref input);

            List<Brep> Breps = new List<Brep>();

            GrasshopperOutput objectList = JsonConvert.DeserializeObject<GrasshopperOutput>(input);
            List<GrasshopperOutputItem> items = objectList.Items;
            if (items != null)
            {
                foreach (GrasshopperOutputItem output in items)
                {
                    switch (output.TypeHint)
                    {
                        case "brep":
                            Brep brep = JsonConvert.DeserializeObject<Brep>(output.Data);
                            Breps.Add(brep.DuplicateBrep()); break;
                    }
                }
            }
            DA.SetDataList(0, Breps);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("523778FB-2B13-4905-BEBB-2CF3347641E0"); }
        }
    }
}
