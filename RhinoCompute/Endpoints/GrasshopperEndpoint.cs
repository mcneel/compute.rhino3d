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
            string body = ctx.GetBody();

            if (string.IsNullOrWhiteSpace(body))
            {
                Response errorResponse = new Response();
                errorResponse.StatusCode = Nancy.HttpStatusCode.BadRequest;
                errorResponse.ReasonPhrase = "No body provided with the request.";
                BH.Engine.RemoteCompute.Log.RecordError(errorResponse.ReasonPhrase);

                return errorResponse;
            }

            ResthopperInput input = JsonConvert.DeserializeObject<ResthopperInput>(body);

            bool usingFormerSchema = false;
            if (input.IsNullOrEmpty())
            {
                input = JsonConvert.DeserializeObject<FormerSchema>(body).ToBHoM();
                usingFormerSchema = !input.IsNullOrEmpty();
            }

            if (input.IsNullOrEmpty())
            {
                Response errorResponse = new Response();
                errorResponse.StatusCode = Nancy.HttpStatusCode.BadRequest;
                errorResponse.ReasonPhrase = "Could not deserialize provided input.";
                BH.Engine.RemoteCompute.Log.RecordError(errorResponse.ReasonPhrase);

                return errorResponse;
            }

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
                    return GrasshopperSolveHelper(input, body, usingFormerSchema);
                }
            else
                return GrasshopperSolveHelper(input, body, usingFormerSchema); // we can't block on recursive calls
        }

        static Response GrasshopperSolveHelper(ResthopperInput resthopperInput, string body, bool usingFormerSchema = false)
        {
            GrasshopperDefinition definition = null;
            if (!resthopperInput.TryCreateGrasshopperDefinition(out definition))
                return null;

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
            string outputSchema_json = "";
            if (!usingFormerSchema)
                outputSchema_json = JsonConvert.SerializeObject(outputSchema, GeometryResolver.JsonSerializerSettings);
            else
                outputSchema_json = JsonConvert.SerializeObject(outputSchema.FromBHoM(), GeometryResolver.JsonSerializerSettings);

            long encodeTime = _stopwatch.ElapsedMilliseconds;

            // Set up response.
            Response outputSchema_nancy = outputSchema_json;
            outputSchema_nancy.ContentType = "application/json";
            outputSchema_nancy = outputSchema_nancy.WithHeader("Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}");

            if (resthopperInput.StoreOutputsInCache)
                DataCache.SetCachedSolveResults(body, outputSchema_json, definition);

            // Clean backend log.
            BH.Engine.RemoteCompute.Log.Clean();

            return outputSchema_nancy;
        }
    }
}
