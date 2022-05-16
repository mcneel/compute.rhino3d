using System;
using BH.Engine.RemoteCompute;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinitionUtils
    {
        public static GrasshopperDefinition FromUrl(Uri scriptUrl, bool storeInCache = true)
        {
            if (scriptUrl == null)
                return null;

            string urlString = scriptUrl.ToString();

            // First check if the definition has been downloaded previously and is present in cache.
            GrasshopperDefinition rc = null;
            if (DataCache.TryGetCachedDefinition(urlString, out rc))
            {
                Serilog.Log.Debug("Using cached definition");
                return rc;
            }

            var archive = BH.Engine.RemoteCompute.RhinoCompute.Compute.GHArchiveFromUrl(scriptUrl);
            if (archive == null)
                return null;

            rc = ConstructAndSetIo(archive);
            rc.CacheKey = urlString;

            if (storeInCache)
                rc.StoredInCache = DataCache.CacheInMemory(urlString, rc);

            return rc;
        }

        public static GrasshopperDefinition FromSingleComponentGuid(Guid componentId, bool cache)
        {
            GrasshopperDefinition rc = ConstructAndSetIo(componentId);

            if (cache)
                rc.StoredInCache = DataCache.CacheInMemory(componentId.ToString(), rc);

            return rc;
        }

    }
}
