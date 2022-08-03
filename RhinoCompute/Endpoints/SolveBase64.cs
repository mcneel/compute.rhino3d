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
        static Response SolveBase64(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out Base64ScriptInput base64Input, out GrasshopperDefinition ghDef, out Response errorResponse))
                return errorResponse;

            // Solve the GrasshopperDefinition.
            ghDef.SolveDefinition();
            ResthopperOutputs resthopperOutput = ghDef.ResthopperOutputs();

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
