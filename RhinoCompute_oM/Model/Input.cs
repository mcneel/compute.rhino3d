using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    [Description("An input parameter of a grasshopper script, potentially with assigned data.")]
    public class Input : IRemoteIO
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IGH_Param Param { get; }
        public GrasshopperDataTree<ResthopperObject> InputData { get; set; }

        public Input(string name, IGH_Param param, string description)
        {
            Name = name;
            Param = param;
            Description = description;
        }
    }
}
