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
        static Response BHoMEndpoint(NancyContext ctx)
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
    }
}
