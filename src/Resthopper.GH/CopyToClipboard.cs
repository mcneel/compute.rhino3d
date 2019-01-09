using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;

namespace Resthopper.GH
{
    public class CopyToClipboard : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CopyToClipboard class.
        /// </summary>
        public CopyToClipboard()
          : base("Copy To Clipboard", "Copy To Clipboard",
              "Description",
              "Resthopper", "Send")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Text", "Text to copy to clipboard", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Result", "Result", "Copy result", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = string.Empty;
            DA.GetData(0, ref text);
            try
            {
                Clipboard.SetText(text);
                DA.SetData(0, true);
            }
            catch
            {
                DA.SetData(0, false);
            }
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
            get { return new Guid("20d1ac1e-2d91-4b61-bff0-383a1a228ce0"); }
        }
    }
}