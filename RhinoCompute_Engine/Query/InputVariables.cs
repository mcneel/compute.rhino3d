using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static List<InputVariable> InputVariables(this GrasshopperDefinition ghDef)
        {
            // Parse input and output names
            var inputs = new List<InputVariable>();
            var sortedInputs = ghDef.Inputs.Values.OrderBy(i => i.Param.Attributes.Pivot.Y);
            foreach (Input input in sortedInputs)
            {
                var inputSchema = new InputVariable
                {
                    Name = input.Name,
                    Description = input.Description(),
                    TypeName = input.Param.ParamTypeNameIncludingSources(), 
                    DefaultValue = input.Param.DefaultValue(),
                    Minimum = input.Param.GetMinimum(),
                    Maximum = input.Param.GetMaximum(),
                    GhAtLeast = input.Param.GetAtLeast(),
                    GhAtMost = input.Param.GetAtMost(),
                };

                if (ghDef.SingularComponent != null)
                {
                    inputSchema.Description = input.Param.Description;
                    if (input.Param.Access == GH_ParamAccess.item)
                        inputSchema.GhAtMost = inputSchema.GhAtLeast;
                }

                inputs.Add(inputSchema);
            }

            return inputs;
        }
    }
}
