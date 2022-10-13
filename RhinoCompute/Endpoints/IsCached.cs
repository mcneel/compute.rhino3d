using System;
using System.Collections.Generic;
using Nancy;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Nancy.Extensions;
using System.Linq;
using BH.oM.Computing;
using BH.Engine.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        Response IsCached(NancyContext ctx, object cacheKeyObj)
        {
            // Obtain the GrasshopperDefinition from body of request.
            string cacheKey = cacheKeyObj.ToString();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return NancyExtensions.CreateErrorResponse("Could not obtain cacheKey from GET request.", HttpStatusCode.BadRequest);

            // Check if the definition has been downloaded previously and is present in cache.
            if (!DataCache.TryGetCachedDefinition(cacheKey, out GrasshopperDefinition definition))
                return NancyExtensions.CreateErrorResponse($"Could not find a definition in Cache under the given cacheKey {cacheKey}.", HttpStatusCode.NotFound);

            return new Response() { StatusCode = HttpStatusCode.OK };
        }
    }
}
