using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static List<OutputVariable> OutputVariables(this GrasshopperDefinition ghDef)
        {
            var outputs = new List<OutputVariable>();
            var sortedOutputs = ghDef.Outputs.Values.OrderBy(i => i.Param.Attributes.Pivot.Y);
            foreach (Output output in sortedOutputs)
            {
                outputs.Add(new OutputVariable
                {
                    Name = output.Name,
                    Description = (output.Param as IGH_ContextualParameter).Description(),
                    TypeName = output.Param.ParamTypeNameIncludingRecipients(),
                    GhNickname = output.Param.NickName,
                });

            }

            return outputs;
        }
    }
}
