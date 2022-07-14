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
        static Response SolveUrl(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out ScriptUrlInput urlInput, out GrasshopperDefinition definition, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ResthopperOutputs outputSchema = definition.SolveDefinition(urlInput.RecursionLevel);

            // Store in cache if required.
            if (urlInput.CacheToDisk)
            {
                if (DataCache.TryWriteToDisk(definition, out string cacheKey))
                    outputSchema.ScriptCacheKey = cacheKey;
            }

            if (urlInput.CacheToMemory)
            {
                if (DataCache.TryWriteInMemory(definition, out string cacheKey))
                    outputSchema.ScriptCacheKey = cacheKey;
            }

            // Set up response.
            Response outputSchema_nancy = outputSchema.ToResponse();

            // Clean backend log.
            BH.Engine.RemoteCompute.Log.Clean();

            return outputSchema_nancy;
        }
    }
}
