using System.Collections.Generic;
using System.Linq;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static List<InputVariable> InputVariables(this GrasshopperDefinition ghDef)
        {
            // Parse input and output names
            var inputs = new List<InputVariable>();
            var sortedInputs = ghDef.Inputs.Values.OrderBy(i => i.Data.Attributes.Pivot.Y);
            foreach (Input input in sortedInputs)
            {
                var inputSchema = new InputVariable
                {
                    Name = input.Name,
                    Description = input.Description(),
                    TypeName = input.Data.ParamTypeNameIncludingSources(), 
                    DefaultValue = input.Data.DefaultValue(),
                    Minimum = input.Data.GetMinimum(),
                    Maximum = input.Data.GetMaximum(),
                    GhAtLeast = input.Data.GetAtLeast(),
                    GhAtMost = input.Data.GetAtMost(),
                };

                if (ghDef.SingularComponent != null)
                {
                    inputSchema.Description = input.Data.Description;
                    if (input.Data.Access == GH_ParamAccess.item)
                        inputSchema.GhAtMost = inputSchema.GhAtLeast;
                }

                inputs.Add(inputSchema);
            }

            return inputs;
        }
    }
}
