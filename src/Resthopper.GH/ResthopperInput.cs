using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Special;
using Resthopper.IO;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;

namespace Resthopper.GH
{
    public class ResthopperInput : GH_Component
    {
        private Resthopper.IO.DataTree<ResthopperObject> InputTree;
        Grasshopper.Kernel.GH_Document Doc;
        /// <summary>
        /// Initializes a new instance of the ResthopperInput class.
        /// </summary>
        public ResthopperInput()
          : base("RH Input", "RH Input",
              "Description",
              "Resthopper", "Send")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("GH Input", "GH Input", "Input Param", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RH Input", "RH Input", "RH Input", GH_ParamAccess.item);
            pManager.AddTextParameter("Param Name", "Param Name", "Param Name", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.Doc = OnPingDocument();
            var inputParam = this.Doc.FindObject(this.Params.Input[0].InstanceGuid, true);
            string groupName = string.Empty;

            foreach (GH_DocumentObject obj in this.Doc.Objects)
            {
                var group = obj as GH_Group;
                if (group == null) { continue; }
                var objectIds = group.Objects()[0].InstanceGuid;
                if (group.Objects()[0].InstanceGuid == this.Params.Input[0].Sources[0].InstanceGuid)
                {
                    groupName = group.NickName;
                }
            }

            if (groupName != string.Empty)
            {
                GHTypeCodes code = (GHTypeCodes)Int32.Parse(groupName.Split(':')[1]);
                Grasshopper.Kernel.Data.GH_Structure<IGH_Goo> booleanInput = null;
                DA.GetDataTree(0, out booleanInput);

                this.InputTree = new Resthopper.IO.DataTree<ResthopperObject>();
                //this.InputTree.ParamName = groupName;
                for (int p = 0; p < booleanInput.PathCount; p++)
                {
                    List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in booleanInput.get_Branch(p))
                    {
                        if (goo == null) continue;
                        else if (goo.GetType() == typeof(GH_Boolean)) { ResthopperObjectList.Add(GetResthopperObject<GH_Boolean>(goo)); }
                        else if (goo.GetType() == typeof(GH_Point)) { ResthopperObjectList.Add(GetResthopperObject<GH_Point>(goo)); }
                        else if (goo.GetType() == typeof(GH_Vector)) { ResthopperObjectList.Add(GetResthopperObject<GH_Vector>(goo)); }
                        else if (goo.GetType() == typeof(GH_Integer)) { ResthopperObjectList.Add(GetResthopperObject<GH_Integer>(goo)); }
                        else if (goo.GetType() == typeof(GH_Number)) { ResthopperObjectList.Add(GetResthopperObject<GH_Number>(goo)); }
                        else if (goo.GetType() == typeof(GH_String)) { ResthopperObjectList.Add(GetResthopperObject<GH_String>(goo)); }
                        else if (goo.GetType() == typeof(GH_Line)) { ResthopperObjectList.Add(GetResthopperObject<GH_Line>(goo)); }
                        else if (goo.GetType() == typeof(GH_Curve)) { ResthopperObjectList.Add(GetResthopperObject<GH_Curve>(goo)); }
                        else if (goo.GetType() == typeof(GH_Circle)) { ResthopperObjectList.Add(GetResthopperObject<GH_Circle>(goo)); }
                        else if (goo.GetType() == typeof(GH_Plane)) { ResthopperObjectList.Add(GetResthopperObject<GH_Plane>(goo)); }
                        else if (goo.GetType() == typeof(GH_Rectangle)) { ResthopperObjectList.Add(GetResthopperObject<GH_Rectangle>(goo)); }
                        else if (goo.GetType() == typeof(GH_Box)) { ResthopperObjectList.Add(GetResthopperObject<GH_Box>(goo)); }
                        else if (goo.GetType() == typeof(GH_Surface)) { ResthopperObjectList.Add(GetResthopperObject<GH_Surface>(goo)); }
                        else if (goo.GetType() == typeof(GH_Brep)) { ResthopperObjectList.Add(GetResthopperObject<GH_Brep>(goo)); }
                        else if (goo.GetType() == typeof(GH_Mesh)) { ResthopperObjectList.Add(GetResthopperObject<GH_Mesh>(goo)); }
                    }

                    GhPath path = new GhPath(new int[] { p });
                    this.InputTree.Add(path, ResthopperObjectList);
                }
                DA.SetData(0, this.InputTree);
                DA.SetData(1, groupName);

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
            get { return new Guid("f82eaed1-8d62-459c-bcbe-11db3725dd5f"); }
        }

        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;
            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v);
            return rhObj;
        }
    }
}