using System;
using System.Collections.Generic;
using Nancy;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Nancy.Extensions;
using System.Linq;
using BH.oM.RemoteCompute;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        Response IoBase64(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            GrasshopperDefinition definition = null;

            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out Base64ScriptInput base64Input, out definition, out Response errorResponse) || definition == null)
                return errorResponse;

            return ResthopperIOVariables(definition).ToResponse();
        }

        Response IoUrl(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            GrasshopperDefinition definition = null;

            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out ScriptUrlInput urlinput, out definition, out Response errorResponse) || definition == null)
                return errorResponse;

            return ResthopperIOVariables(definition).ToResponse();
        }

        Response IoCacheKey(NancyContext ctx)
        {
            // Obtain the GrasshopperDefinition from body of request.
            GrasshopperDefinition definition = null;

            if (!ctx.TryDeserializeAndGetGrasshopperDefinition(out CacheKeyInput cacheKeyInput, out definition, out Response errorResponse) || definition == null)
                return errorResponse;

            return NancyExtensions.CreateErrorResponse("Could not extract Inputs/Outputs.");
        }

        private ResthopperIOVariables ResthopperIOVariables(GrasshopperDefinition ghDef)
        {
            return new ResthopperIOVariables
            {
                Description = ghDef.SingularComponent?.Description ?? ghDef.GH_Document.Properties.Description,
                Inputs = ghDef.InputVariables(),
                Outputs = ghDef.OutputVariables()
            };
        }
    }
}
