using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using System.Linq;
using BH.oM.RemoteCompute;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public class InputGroup
    {
        public IGH_Param Param { get; }
        public GrasshopperDataTree<ResthopperObject> DataTree { get; set; }

        public InputGroup(IGH_Param param)
        {
            Param = param;
        }
    }
}
