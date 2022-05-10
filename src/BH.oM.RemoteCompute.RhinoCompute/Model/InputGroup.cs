using Grasshopper.Kernel;

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
