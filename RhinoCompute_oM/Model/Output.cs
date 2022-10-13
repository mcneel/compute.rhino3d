using System.ComponentModel;
using Grasshopper.Kernel;

namespace BH.oM.Computing.RhinoCompute
{
    [Description("An output variable of a grasshopper script. Also acts as a data object that can host the results of a computation (set into the IGH_Param).")]
    public class Output : IRemoteIOVariable, IRemoteIOData<IGH_Param>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TypeName { get { return Data.TypeName; } set { TypeName = value; } }

        public IGH_Param Data { get; set; }

        public Output(string name, IGH_Param param, string description)
        {
            Name = name;
            Data = param;
            Description = description;
        }
    }
}
