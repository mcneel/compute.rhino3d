using System.Collections.Generic;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public partial class GrasshopperDefinition : IGrasshopperDefinition
    {
        public GrasshopperDefinition(GH_Document gh_document)
        {
            GH_Document = gh_document;
        }

        public bool IsSolved { get; set; } = false;

        public Dictionary<string, Input> Inputs { get; set; } = new Dictionary<string, Input>();
        public Dictionary<string, Output> Outputs { get; set; } = new Dictionary<string, Output>();

        public GH_Document GH_Document { get; }
        public GH_Component SingularComponent { get; set; } = null;
    }
}
