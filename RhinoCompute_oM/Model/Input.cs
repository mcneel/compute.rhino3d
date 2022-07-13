using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    [Description("An input parameter of a grasshopper script, potentially with assigned data.")]
    public class Input
    {
        public string Name { get; }
        public IGH_Param Param { get; }
        public GrasshopperDataTree<ResthopperObject> InputData { get; set; }

        public Input(string name, IGH_Param param)
        {
            Name = name;
            Param = param;
        }
    }
}
