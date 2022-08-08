using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    [Description("An output variable of a grasshopper script. Also acts as a data object that can host the results of a computation (set into the IGH_Param).")]
    public class Output : IRemoteIOVariable, IRemoteIOData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TypeName { get { return Param.TypeName; } set { TypeName = value; } }

        public IGH_Param Param { get; }

        public Output(string name, IGH_Param param, string description)
        {
            Name = name;
            Param = param;
            Description = description;
        }
    }
}
