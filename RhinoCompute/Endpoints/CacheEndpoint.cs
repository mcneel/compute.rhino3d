using System;
using Nancy;
using Newtonsoft.Json;
using Nancy.Extensions;
using BH.oM.RemoteCompute;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using BH.Engine.RemoteCompute;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        static Response SolveCache(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out CacheKeyInput cacheInput, out GrasshopperDefinition definition, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ResthopperOutputs outputSchema = definition.SolveDefinition(cacheInput.RecursionLevel);

            // Set up response.
            Response outputSchema_nancy = outputSchema.ToResponse();

            // Clean backend log.
            Log.Clean();

            return outputSchema_nancy;
        }
    }
}
