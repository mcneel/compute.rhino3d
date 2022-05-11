using System;
using System.Collections.Generic;
using Nancy;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Resthopper.IO;
using Nancy.Extensions;
using System.Linq;
using BH.oM.RemoteCompute;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        Response IOsEndpoint(NancyContext ctx, bool asPost)
        {
            GrasshopperDefinition definition;

            if (asPost)
            {
                string body = ctx.Request.Body.AsString();
                if (body.StartsWith("[") && body.EndsWith("]"))
                    body = body.Substring(1, body.Length - 2);

                ResthopperInput input = JsonConvert.DeserializeObject<ResthopperInput>(body);

                // load grasshopper file
                definition = GrasshopperDefinitionUtils.FromUrl(new Uri(input.Script));
                if (definition == null)
                    definition = GrasshopperDefinitionUtils.FromBase64String(input.Script);
            }
            else
            {
                string url = Request.Query[nameof(ResthopperInput.Script)].ToString();
                definition = GrasshopperDefinitionUtils.FromUrl(new Uri(url));
            }

            if (definition == null)
                throw new Exception("Unable to load grasshopper definition");

            IoResponse ioResponse = IoResponse(definition);
            ioResponse.CacheKey = definition.CacheKey;

            foreach (var error in definition.Errors)
                ioResponse.Errors.Add(error);

            foreach (var error in Logging.Errors)
                ioResponse.Errors.Add(error);

            string ioResponse_json = JsonConvert.SerializeObject(ioResponse);
            Response ioResponse_json_nancy = ioResponse_json;
            ioResponse_json_nancy.ContentType = "application/json";

            Logging.Warnings.Clear();
            Logging.Errors.Clear();

            return ioResponse_json_nancy;
        }


        private IoResponse IoResponse(GrasshopperDefinition ghDef)
        {
            // Parse input and output names
            List<string> inputNames = new List<string>();
            List<string> outputNames = new List<string>();
            var inputs = new List<InputParam>();
            var outputs = new List<IoParam>();

            IOrderedEnumerable<KeyValuePair<string, InputGroup>> sortedInputs = ghDef.Inputs.OrderBy(i => i.Value.Param.Attributes.Pivot.Y);
            IOrderedEnumerable<KeyValuePair<string, IGH_Param>> sortedOutputs = ghDef.Outputs.OrderBy(i => i.Value.Attributes.Pivot.Y);

            foreach (var input in sortedInputs)
            {
                inputNames.Add(input.Key);
                var inputSchema = new InputParam
                {
                    Name = input.Key,
                    ParamTypeName = input.Value.Param.ParamTypeName(),
                    Description = input.Value.Description(),
                    AtLeast = input.Value.GetAtLeast(),
                    AtMost = input.Value.GetAtMost(),
                    Default = input.Value.DefaultValue(),
                    Minimum = input.Value.GetMinimum(),
                    Maximum = input.Value.GetMaximum(),
                };

                if (ghDef.SingularComponent != null)
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

            string description = ghDef.SingularComponent?.Description ?? ghDef.GH_Document.Properties.Description;

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
