using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    [Description("An output parameter of a grasshopper script, potentially with assigned data.")]
    public class Output
    {
        public string Name { get; }
        public IGH_Param Param { get; }

        public Output(string name, IGH_Param param)
        {
            Name = name;
            Param = param;
        }
    }
}
