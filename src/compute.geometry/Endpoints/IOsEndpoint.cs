using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Grasshopper.Kernel.Data;
using Resthopper.IO;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Net;
using Nancy.Extensions;
using System.Reflection;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using static compute.geometry.GrasshopperDefinition;
using BH.Engine.RemoteCompute.RhinoCompute;

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

                FullRhinoComputeSchema input = JsonConvert.DeserializeObject<FullRhinoComputeSchema>(body);

                // load grasshopper file
                definition = GrasshopperDefinition.FromUrl(input.Pointer, true);
                if (definition == null)
                {
                    definition = GrasshopperDefinition.FromBase64String(input.Algo, true);
                }
            }
            else
            {
                string url = Request.Query["Pointer"].ToString();
                definition = GrasshopperDefinition.FromUrl(url, true);
            }

            if (definition == null)
                throw new Exception("Unable to load grasshopper definition");

            IoResponse ioResponse = IoResponse(definition);
            ioResponse.CacheKey = definition.CacheKey;

            foreach (var error in definition.ErrorMessages)
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
                    Description = input.Value.GetDescription(),
                    AtLeast = input.Value.GetAtLeast(),
                    AtMost = input.Value.GetAtMost(),
                    Default = input.Value.GetDefault(),
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
