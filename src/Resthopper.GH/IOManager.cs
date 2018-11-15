using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Resthopper.IO;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ResthopperGH
{
    public class IOManager : GH_Component
    {
        private GH_Document doc;
        private IGH_Component Component;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public IOManager()
          : base("IO Manager", "IO Manager",
              "Description",
              "Resthopper", "Send")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Set Inputs", "Set Inputs", "Select the input params in your Grasshopper definition and click this button to set them as Resthopper inputs", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Set Outputs", "Set Outputs", "Select the output params in your Grasshopper definition and click this button to set them as Resthopper outputs", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Inputs", "Inputs", "List of set inputs", GH_ParamAccess.list);
            pManager.AddTextParameter("Outputs", "Outputs", "List of set outputs", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool inputButton = false;
            bool outputButton = false;

            DA.GetData<bool>(0, ref inputButton);
            DA.GetData<bool>(1, ref outputButton);

            this.doc = this.OnPingDocument();
            this.Component = this;

            List<IGH_DocumentObject> SelecteedObjects = new List<IGH_DocumentObject>();
            List<string> inputList = new List<string>();
            List<string> outputList = new List<string>();

            if (inputButton || outputButton)
            {
                foreach (IGH_DocumentObject obj in this.doc.Objects)
                {
                    if (obj.Attributes.Selected && ValidateType(obj)) { SelecteedObjects.Add(obj); }
                }
            }

            if (inputButton)
            {
                foreach (IGH_DocumentObject obj in SelecteedObjects)
                {
                    Grasshopper.Kernel.Special.GH_Group group = new Grasshopper.Kernel.Special.GH_Group();
                    group.AddObject(obj.InstanceGuid);
                    int code = GetGrasshopperTypeCode(obj);
                    group.NickName = $"RH_IN:{code}";
                    group.Colour = System.Drawing.Color.Cyan;
                    inputList.Add(obj.InstanceGuid.ToString());
                    this.doc.AddObject(group, false);
                }
            }

            if (outputButton)
            {
                foreach (IGH_DocumentObject obj in SelecteedObjects)
                {
                    Grasshopper.Kernel.Special.GH_Group group = new Grasshopper.Kernel.Special.GH_Group();
                    group.AddObject(obj.InstanceGuid);
                    int code = GetGrasshopperTypeCode(obj);
                    group.NickName = $"RH_OUT:{code}";
                    group.Colour = System.Drawing.Color.LightGreen;
                    outputList.Add(obj.InstanceGuid.ToString());
                    this.doc.AddObject(group, false);
                }
            }

            DA.SetDataList(0, inputList);
            DA.SetDataList(1, outputList);




        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        private bool ValidateType(IGH_DocumentObject obj)
        {
            if (obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_BooleanToggle) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_NumberSlider) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_Panel) ||

                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Boolean) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Point) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Vector) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Integer) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Number) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_String) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Line) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Curve) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Circle) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Plane) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Rectangle) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Box) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Surface) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Brep) ||
                obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Mesh))
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private int GetGrasshopperTypeCode(IGH_DocumentObject obj)
        {
            if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Boolean)) { return (int)GHTypeCodes.Boolean; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Point)) { return (int)GHTypeCodes.Point; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Vector)) { return (int)GHTypeCodes.Vector; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Integer)) { return (int)GHTypeCodes.Integer; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Number)) { return (int)GHTypeCodes.Number; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_String)) { return (int)GHTypeCodes.Text; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Line)) { return (int)GHTypeCodes.Line; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Curve)) { return (int)GHTypeCodes.Curve; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Circle)) { return (int)GHTypeCodes.Circle; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Plane)) { return (int)GHTypeCodes.PLane; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Rectangle)) { return (int)GHTypeCodes.Rectangle; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Box)) { return (int)GHTypeCodes.Box; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Surface)) { return (int)GHTypeCodes.Surface; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Brep)) { return (int)GHTypeCodes.Brep; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Parameters.Param_Mesh)) { return (int)GHTypeCodes.Mesh; }

            else if (obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_NumberSlider)) { return (int)GHTypeCodes.Slider; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_BooleanToggle)) { return (int)GHTypeCodes.BooleanToggle; }
            else if (obj.GetType() == typeof(Grasshopper.Kernel.Special.GH_Panel)) { return (int)GHTypeCodes.Panel; }
            
            return 0;
        }
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
            get { return new Guid("c76c29c3-bc7f-4317-b8a7-d8e2f1e80002"); }
        }
    }
}
