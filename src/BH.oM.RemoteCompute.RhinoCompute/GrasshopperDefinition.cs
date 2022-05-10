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

        public Dictionary<string, InputGroup> Inputs { get; set; } = new Dictionary<string, InputGroup>();
        public Dictionary<string, IGH_Param> Outputs { get; set; } = new Dictionary<string, IGH_Param>();

        public GH_Document GH_Document { get; }
        public GH_Component SingularComponent { get; set; } = null;

        public string CacheKey { get; set; }
        public bool StoredInCache { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Remarks { get; set; } = new List<string>();
    }
}
