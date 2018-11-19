using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Resthopper.IO;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace Resthopper.GH
{
    public class ResthopperClient : GH_Component
    {
        
        private Schema OutputSchema = new Schema();
        /// <summary>
        /// Initializes a new instance of the ResthopperClient class.
        /// </summary>
        public ResthopperClient()
          : base("RH Client", "RH Client",
              "Description",
              "Resthopper", "Send")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("path", "path", "path to the GHX file", GH_ParamAccess.item);
            pManager.AddTextParameter("token", "token", "Resthopper Auth token", GH_ParamAccess.item);
            pManager.AddTextParameter("server", "server", "Server IP or URL", GH_ParamAccess.item);
            pManager.AddBooleanParameter("run", "run", "Send to Resthopper", GH_ParamAccess.item);
            pManager.AddGenericParameter("inputs", "inputs", "List of ResthopperObjects", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Schema", "Schema", "Resthopper Schema object", GH_ParamAccess.item);
            pManager.AddTextParameter("json", "json", "json", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected async override void SolveInstance(IGH_DataAccess DA)
        {
            string path = string.Empty;
            string token = string.Empty;
            string server = string.Empty;
            bool run = false;
            List<Resthopper.IO.DataTree<ResthopperObject>> inputs = new List<Resthopper.IO.DataTree<ResthopperObject>>();
            List<string> paramNames = new List<string>();

            DA.GetData(0, ref path);
            DA.GetData(1, ref token);
            DA.GetData(2, ref server);
            DA.GetData(3, ref run);
            DA.GetDataList(4, inputs);


            if (run)
            {
                HttpClient client = new HttpClient();
                Schema InputSchema = new Schema();
                string markup;
                using (StreamReader reader = new StreamReader(path))
                { markup = reader.ReadToEnd(); }
                InputSchema.Algo = Base64Encode(markup);
                InputSchema.Values = inputs;

                JsonSerializerSettings settings = new JsonSerializerSettings();
                //settings.ContractResolver = new DictionaryAsArrayResolver();
                settings.Formatting = Formatting.Indented;
                DA.SetData(1, JsonConvert.SerializeObject(InputSchema, settings));

                //Schema output = await ResthopperPipeline.Request(InputSchema, server, token);
                //this.OutputSchema = output;

                //DA.SetData(0, this.OutputSchema);
                

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
            get { return new Guid("0d9543bf-0c48-4527-b25b-05d9e7e29f69"); }
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}