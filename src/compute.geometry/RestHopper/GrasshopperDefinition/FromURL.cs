using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public static GrasshopperDefinition FromUrl(Uri scriptUrl, bool cache)
        {
            if (scriptUrl == null)
                return null;

            string urlString = scriptUrl.ToString();

            // First check if the definition has been downloaded previously and is present in cache.
            GrasshopperDefinition rc = null;
            if (DataCache.TryGetCachedDefinition(urlString, out rc))
            {
                LogDebug("Using cached definition");
                return rc;
            }

            var archive = ArchiveFromUrl(scriptUrl);
            if (archive == null)
                return null;

            rc = ConstructAndSetIo(archive);
            rc.CacheKey = urlString;
            rc.IsLocalFileDefinition = !urlString.StartsWith("http", StringComparison.OrdinalIgnoreCase) && File.Exists(urlString);

            if (cache)
                rc.StoredInCache = DataCache.CacheInMemory(urlString, rc);

            return rc;
        }

        public static GrasshopperDefinition FromSingleComponentGuid(Guid componentId, bool cache)
        {
            GrasshopperDefinition rc = Construct(componentId);

            if (cache)
                rc.StoredInCache = DataCache.CacheInMemory(componentId.ToString(), rc);

            return rc;
        }

    }
}
