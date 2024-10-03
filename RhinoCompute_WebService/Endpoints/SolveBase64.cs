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
        static Response SolveBase64(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out Base64ScriptInput base64Input, out GrasshopperDefinition ghDef, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ResthopperOutputs resthopperOutput = ghDef.SolveAndGetOutputs();

            // Store in cache if required.
            if (base64Input.CacheToDisk)
            {
                if (DataCache.TryWriteToDisk(base64Input.Base64Script, out string cacheKey))
                    resthopperOutput.ScriptCacheKey = cacheKey;
            }

            if (base64Input.CacheToMemory)
            {
                if (DataCache.TryWriteInMemory(ghDef, out string cacheKey))
                    resthopperOutput.ScriptCacheKey = cacheKey;
            }

            // Set up response.
            Response outputSchema_nancy = resthopperOutput.ToResponse();

            // Clean backend log.
            Log.Clean();

            return outputSchema_nancy;
        }
    }
}
