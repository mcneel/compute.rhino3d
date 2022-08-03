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
using System.Collections.Generic;
using BH.oM.RemoteCompute.Schemas;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        /// <summary>
        /// Endpoint for compatibility with existing Rhino.Compute scripts.
        /// </summary>
        static Response GrasshopperEndpoint(NancyContext ctx)
        {
            if (!ctx.TryDeserializeToInput(out FormerRestSchema formerRestSchema, out Response errorResponse))
                return errorResponse;

            ResthopperInputs convertedSchema = formerRestSchema.ToBHoM();

            GrasshopperDefinition ghDef = null;
            ResthopperOutputs resthopperOutput = null;
            errorResponse = null;
            if (!convertedSchema.ITryCreateGrasshopperDefinition(out ghDef))
            {
                errorResponse = NancyExtensions.CreateErrorResponse("Could not create Grasshopper Definition from the provided input.", HttpStatusCode.BadRequest);
                return errorResponse;
            }

            ghDef.SetInputsData(convertedSchema.InputsData);

            // Solve definition.
            ghDef.SolveDefinition();
            resthopperOutput = ghDef.ResthopperOutputs();

            // Cache to disk if possible and if required.
            if (convertedSchema is ICacheable cacheable)
            {
                if ((cacheable.CacheToMemory && DataCache.TryWriteInMemory(ghDef, out string cacheKey)) || 
                    (cacheable.CacheToDisk && DataCache.TryWriteToDisk(ghDef, out cacheKey)))
                    resthopperOutput.ScriptCacheKey = cacheKey;
            }

            // Create output response.
            FormerRestSchema formerSchemaOutput = resthopperOutput.FromBHoM();
            Response response = formerSchemaOutput.ToResponse();

            // Clean backend log.
            BH.Engine.RemoteCompute.Log.Clean();

            return response;
        }
    }
}
