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
        static Response SolveUrl(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out ScriptUrlInput urlInput, out GrasshopperDefinition ghDef, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ResthopperOutputs resthopperOutput = ghDef.SolveAndGetOutputs();

            // Store in cache if required.
            if (urlInput.CacheToDisk)
            {
                if (DataCache.TryWriteToDisk(ghDef, out string cacheKey))
                    resthopperOutput.ScriptCacheKey = cacheKey;
            }

            if (urlInput.CacheToMemory)
            {
                if (DataCache.TryWriteInMemory(ghDef, out string cacheKey))
                    resthopperOutput.ScriptCacheKey = cacheKey;
            }

            // Set up response.
            Response outputSchema_nancy = resthopperOutput.ToResponse();

            // Clean backend log.
            BH.Engine.Computing.Log.Clean();

            return outputSchema_nancy;
        }
    }
}
