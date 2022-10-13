using System.Collections.Generic;
using BH.Engine.Computing;
using BH.Engine.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;
using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;

namespace compute.geometry
{
    public static class NancyExtensions
    {
        public static bool TryGetBody(this NancyContext ctx, out string body, out Response errorResponse)
        {
            body = null;
            errorResponse = null;

            try
            {
                body = ctx.Request.Body.AsString();
                if (body.StartsWith("[") && body.EndsWith("]"))
                    body = body.Substring(1, body.Length - 2);

                return true;
            }
            catch { }

            if (string.IsNullOrWhiteSpace(body))
            {
                errorResponse = CreateErrorResponse("No body provided with the request.", HttpStatusCode.BadRequest);
                return false;
            }

            return false;
        }

        public static bool TryDeserializeToInput<T>(this NancyContext ctx, out T deserialized, out Response errorResponse) where T : class
        {
            deserialized = default(T);
            errorResponse = default(Response);

            if (!ctx.TryGetBody(out string body, out errorResponse))
                return false;

            deserialized = JsonConvert.DeserializeObject<T>(body);

            if (deserialized.IsNullOrEmpty())
            {
                errorResponse = CreateErrorResponse("Could not deserialize provided input.", HttpStatusCode.BadRequest);
                return false;
            }

            return true;
        }

        public static Response CreateErrorResponse(string reasonPhrase, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, bool logError = true)
        {
            Response errorResponse = new Response();
            errorResponse.StatusCode = statusCode;
            errorResponse.ReasonPhrase = reasonPhrase;

            if (logError)
                BH.Engine.Computing.Log.RecordError(reasonPhrase);

            return errorResponse;
        }



        public static Response ToResponse(this IRhinoComputeSchema resthopperOutput, Dictionary<string, string> additionalHeaderInfo = null)
        {
            // Serialize result.
            string outputSchema_json = "";
            outputSchema_json = JsonConvert.SerializeObject(resthopperOutput, GeometryResolver.JsonSerializerSettings);

            // Set up response.
            Response outputSchema_nancy = outputSchema_json;
            outputSchema_nancy.ContentType = "application/json";

            if (additionalHeaderInfo != null)
                foreach (var item in additionalHeaderInfo)
                {
                    // Add headers, e.g.:
                    // "Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}"
                    outputSchema_nancy = outputSchema_nancy.WithHeader(item.Key, item.Value);
                }

            return outputSchema_nancy;
        }

        public static Response ToResponse(this FormerRestSchema formerSchemaOutput, Dictionary<string, string> additionalHeaderInfo = null)
        {
            string outputSchema_json = "";

            outputSchema_json = JsonConvert.SerializeObject(formerSchemaOutput, GeometryResolver.JsonSerializerSettings);

            // Set up response.
            Response outputSchema_nancy = outputSchema_json;
            outputSchema_nancy.ContentType = "application/json";

            if (additionalHeaderInfo != null)
                foreach (var item in additionalHeaderInfo)
                {
                    // Add headers, e.g.:
                    // "Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}"
                    outputSchema_nancy = outputSchema_nancy.WithHeader(item.Key, item.Value);
                }

            return outputSchema_nancy;
        }

        public static bool TryDeserializeAndGetGrasshopperDefinition<T>(this NancyContext ctx, out T resthopperInputs, out GrasshopperDefinition grasshopperDefinition, out Response errorResponse, bool logErrors = true) where T : ResthopperInputs
        {
            grasshopperDefinition = null;
            resthopperInputs = null;

            // Extract the body from the request.
            if (!ctx.TryGetBody(out string body, out errorResponse))
                return false;

            // Convert the Body into an input schema.
            resthopperInputs = JsonConvert.DeserializeObject<T>(body);

            if (resthopperInputs.IsNullOrEmpty())
            {
                errorResponse = NancyExtensions.CreateErrorResponse("Could not deserialize provided input.", HttpStatusCode.BadRequest, logErrors);
                return false;
            }

            // Convert the input schema to a GrasshopperDefinition object.
            if (!resthopperInputs.ITryCreateGrasshopperDefinition(out grasshopperDefinition))
            {
                errorResponse = CreateErrorResponse("Could not create Grasshopper definition from imput data.", HttpStatusCode.InternalServerError, logErrors);
                return false;
            }

            // Assign inputs data.
            grasshopperDefinition.SetInputsData(resthopperInputs.InputsData);

            return true;
        }
    }
}
