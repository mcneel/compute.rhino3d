using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    [Description("An input variable of a grasshopper script. Also acts as a data object that can host the input data for a computation.")]
    public class Input : IRemoteIOVariable, IRemoteIOData
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IGH_Param Param { get; }
        public GrasshopperDataTree<ResthopperObject> InputData { get; set; }
        public string TypeName { get { return Param.TypeName; } set { TypeName = value; } }

        public Input(string name, IGH_Param param, string description)
        {
            Name = name;
            Param = param;
            Description = description;
        }
    }
}
