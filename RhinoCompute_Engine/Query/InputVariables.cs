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
                    ParamTypeName = input.Param.ParamTypeName(),
                    Description = (input as IGH_ContextualParameter).Description(),
                    AtLeast = input.Param.GetAtLeast(),
                    AtMost = input.Param.GetAtMost(),
                    DefaultValue = new List<List<object>>() { new List<object>() { input.Param.DefaultValue() } },
                    Minimum = input.Param.GetMinimum(),
                    Maximum = input.Param.GetMaximum(),
                };

                if (ghDef.SingularComponent != null)
                {
                    inputSchema.Description = input.Param.Description;
                    if (input.Param.Access == GH_ParamAccess.item)
                    {
                        inputSchema.AtMost = inputSchema.AtLeast;
                    }
                }

                inputs.Add(inputSchema);
            }

            return inputs;
        }
    }
}
