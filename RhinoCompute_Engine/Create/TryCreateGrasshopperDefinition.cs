using System;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Create
    {
        public static bool ITryCreateGrasshopperDefinition(this IRhinoComputeSchema resthopperInput, out GrasshopperDefinition definition)
        {
            return TryCreateGrasshopperDefinition(resthopperInput as dynamic, out definition);
        }

        public static bool TryCreateGrasshopperDefinition(this Base64ScriptInput resthopperInput, out GrasshopperDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(resthopperInput.Base64Script))
            {
                Log.RecordError("Missing script input.");
                return false;
            }

            definition = Create.GrasshopperDefinitionFromBase64String(resthopperInput.Base64Script);

            if (definition == null)
                Log.RecordError("Unable to convert Base-64 encoded Grasshopper script to a GrasshopperDefinition object.");

            return definition != null;
        }

        public static bool TryCreateGrasshopperDefinition(this ScriptUrlInput resthopperInput, out GrasshopperDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(resthopperInput.ScriptUrl.ToString()))
            {
                Log.RecordError("Missing script input.");
                return false;
            }

            definition = Create.GrasshopperDefinitionFromUri(resthopperInput.ScriptUrl);

            return definition != null;
        }

        public static bool TryCreateGrasshopperDefinition(this CacheKeyInput resthopperInput, out GrasshopperDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(resthopperInput.CacheKey))
            {
                Log.RecordError("Missing script input.");
                return false;
            }

            // First check if the definition has been downloaded previously and is present in cache.
            if (DataCache.TryGetCachedDefinition(resthopperInput.CacheKey, out definition))
                return true;

            Log.RecordError("Could not find definition in cache.");
            return false;
        }
    }
}
