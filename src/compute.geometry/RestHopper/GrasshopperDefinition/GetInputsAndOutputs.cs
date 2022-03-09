using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public IoResponse GetInputsAndOutputs()
        {
            // Parse input and output names
            List<string> inputNames = new List<string>();
            List<string> outputNames = new List<string>();
            var inputs = new List<InputParam>();
            var outputs = new List<IoParam>();

            var sortedInputs = from x in _input orderby x.Value.Param.Attributes.Pivot.Y select x;
            var sortedOutputs = from x in _output orderby x.Value.Attributes.Pivot.Y select x;

            foreach (var input in sortedInputs)
            {
                inputNames.Add(input.Key);
                var inputSchema = new InputParam
                {
                    Name = input.Key,
                    ParamTypeName = input.Value.Param.ParamTypeName(),
                    Description = input.Value.GetDescription(),
                    AtLeast = input.Value.GetAtLeast(),
                    AtMost = input.Value.GetAtMost(),
                    Default = input.Value.GetDefault(),
                    Minimum = input.Value.GetMinimum(),
                    Maximum = input.Value.GetMaximum(),
                };
                if (_singularComponent != null)
                {
                    inputSchema.Description = input.Value.Param.Description;
                    if (input.Value.Param.Access == GH_ParamAccess.item)
                    {
                        inputSchema.AtMost = inputSchema.AtLeast;
                    }
                }
                inputs.Add(inputSchema);
            }

            foreach (var o in sortedOutputs)
            {
                outputNames.Add(o.Key);
                outputs.Add(new IoParam
                {
                    Name = o.Key,
                    ParamTypeName = o.Value.TypeName
                });
            }

            string description = _singularComponent == null ?
                GH_Document.Properties.Description :
                _singularComponent.Description;

            return new IoResponse
            {
                Description = description,
                InputNames = inputNames,
                OutputNames = outputNames,
                Inputs = inputs,
                Outputs = outputs
            };
        }
    }
}
