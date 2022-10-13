using System.Collections.Generic;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public partial class GrasshopperDefinition : IGrasshopperDefinition
    {
        public GrasshopperDefinition(GH_Document gh_document, GHScriptConfig ghScriptConfig)
        {
            GH_Document = gh_document;
            GHScriptConfig = ghScriptConfig;
        }

        public bool IsSolved { get; set; } = false;

        public Dictionary<string, Input> Inputs { get; set; } = new Dictionary<string, Input>();
        public Dictionary<string, Output> Outputs { get; set; } = new Dictionary<string, Output>();

        public SortedDictionary<int, IGH_DocumentObject> Triggers { get; set; } = new SortedDictionary<int, IGH_DocumentObject>();


        public GH_Document GH_Document { get; }
        public GH_Component SingularComponent { get; set; } = null;
        public GHScriptConfig GHScriptConfig { get; }
    }
}
