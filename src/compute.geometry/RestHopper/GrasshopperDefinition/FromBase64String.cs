using BH.Engine.RemoteCompute.RhinoCompute;
using BH.Engine.RemoteCompute.RhinoCompute.Objects;

namespace compute.geometry
{
    partial class GrasshopperDefinitionUtils
    {
        public static GrasshopperDefinition FromBase64String(string base64string, bool cacheToDisk = true)
        {
            var archive = base64string.ArchiveFromBase64String();
            if (archive == null)
                return null;

            var rc = ConstructAndSetIo(archive);

            if (rc != null)
            {
                rc.CacheKey = DataCache.CreateCacheKey(base64string);

                if (cacheToDisk)
                {
                    DataCache.CacheToDisk(rc.CacheKey, base64string);
                    rc.StoredInCache = true;
                }
            }

            return rc;
        }
    }
}
