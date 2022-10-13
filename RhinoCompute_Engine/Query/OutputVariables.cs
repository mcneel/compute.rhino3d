using System.Collections.Generic;
using System.Linq;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static List<OutputVariable> OutputVariables(this GrasshopperDefinition ghDef)
        {
            var outputs = new List<OutputVariable>();
            var sortedOutputs = ghDef.Outputs.Values.OrderBy(i => i.Data.Attributes.Pivot.Y);
            foreach (Output output in sortedOutputs)
            {
                outputs.Add(new OutputVariable
                {
                    Name = output.Name,
                    Description = output.Description(),
                    TypeName = output.Data.ParamTypeNameIncludingRecipients(),
                    GhNickname = output.Data.NickName,
                });
            }

            return outputs;
        }
    }
}
