using System;
using Nancy;
using Newtonsoft.Json;
using Nancy.Extensions;
using BH.oM.RemoteCompute;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.Engine.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        static Response GrasshopperEndpoint(NancyContext ctx)
        {
            string body = ctx.Request.Body.AsString();
            if (body.StartsWith("[") && body.EndsWith("]"))
                body = body.Substring(1, body.Length - 2);

            ResthopperInput input = JsonConvert.DeserializeObject<ResthopperInput>(body);

            if (input.StoreOutputsInCache)
            {
                // look in the cache to see if this has already been solved
                string cachedReturnJson = DataCache.GetCachedSolveResults(body);
                if (!string.IsNullOrWhiteSpace(cachedReturnJson))
                {
                    Response cachedResponse = cachedReturnJson;
                    cachedResponse.ContentType = "application/json";
                    return cachedResponse;
                }
            }

            // 5 Feb 2021 S. Baer
            // Throw a lock around the entire solve process for now. I can easily
            // repeat multi-threaded issues by creating a catenary component with Hops
            // that has one point for A and multiple points for B.
            // We can narrow down this lock over time. As it stands, launching many
            // compute instances on one computer is going to be a better solution anyway
            // to deal with solving many times simultaniously.
            if (input.RecursionLevel == 0)
                lock (_ghsolvelock)
                {
                    return GrasshopperSolveHelper(input, body);
                }
            else
                return GrasshopperSolveHelper(input, body); // we can't block on recursive calls
        }

        static Response GrasshopperSolveHelper(ResthopperInput resthopperInput, string body)
        {
            GrasshopperDefinition definition = null;

            if (string.IsNullOrWhiteSpace(resthopperInput.Script))
                throw new Exception("Missing script input.");

            Uri scriptUrl = null;
            if (Uri.TryCreate(resthopperInput.Script, UriKind.Absolute, out scriptUrl))
                definition = GrasshopperDefinitionUtils.FromUrl(scriptUrl);

            if (definition == null)
            {
                definition = GrasshopperDefinitionUtils.FromBase64String(resthopperInput.Script);

                if (definition == null)
                    throw new Exception("Unable to convert Base-64 encoded Grasshopper script to a GrasshopperDefinition object.");
            }

            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int recursionLevel = resthopperInput.RecursionLevel + 1;
            definition.GH_Document.DefineConstant("ComputeRecursionLevel", new Grasshopper.Kernel.Expressions.GH_Variant(recursionLevel));

            definition.AssignInputData(resthopperInput.Data);

            long decodeTime = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();

            // Solve definition.
            ResthopperOutput outputSchema = definition.SolveDefinition();
            long solveTime = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();

            outputSchema.ScriptCacheKey = definition.CacheKey;

            // Serialize result.
            string outputSchema_json = JsonConvert.SerializeObject(outputSchema, GeometryResolver.JsonSerializerSettings);
            long encodeTime = _stopwatch.ElapsedMilliseconds;

            // Set up response.
            Response outputSchema_nancy = outputSchema_json;
            outputSchema_nancy.ContentType = "application/json";
            outputSchema_nancy = outputSchema_nancy.WithHeader("Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}");

            if (definition.Remarks.Any()) // TODO: Errors should be here
            {
                outputSchema_nancy.StatusCode = Nancy.HttpStatusCode.InternalServerError;
                outputSchema_nancy.ReasonPhrase = "Errors:\n\t" + string.Join("\n\t", definition.Remarks); // TODO: Errors should be here
            }
            else
                if (resthopperInput.StoreOutputsInCache)
                DataCache.SetCachedSolveResults(body, outputSchema_json, definition);

            return outputSchema_nancy;
        }
    }
}
