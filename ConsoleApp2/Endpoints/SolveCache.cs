using System;
using Nancy;
using Newtonsoft.Json;
using Nancy.Extensions;
using BH.oM.Computing;
using System.Linq;
using BH.oM.Computing.RhinoCompute;
using BH.Engine.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;
using BH.Engine.Computing;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        static Response SolveCache(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out CacheKeyInput cacheInput, out GrasshopperDefinition ghDef, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ResthopperOutputs resthopperOutput = ghDef.SolveAndGetOutputs();

            // Set up response.
            Response outputSchema_nancy = resthopperOutput.ToResponse();

            // Clean backend log.
            Log.Clean();

            return outputSchema_nancy;
        }
    }
}
