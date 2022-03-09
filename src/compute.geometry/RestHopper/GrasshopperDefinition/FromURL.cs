using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

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
        public static GrasshopperDefinition FromUrl(string url, bool cache)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // First check if the definition has been downloaded previously and is present in cache.
            GrasshopperDefinition rc = DataCache.GetCachedDefinition(url);

            if (rc != null)
            {
                LogDebug("Using cached definition");
                return rc;
            }

            if (Guid.TryParse(url, out Guid componentId))
            {
                rc = Construct(componentId);
            }
            else
            {
                var archive = ArchiveFromUrl(url);
                if (archive == null)
                    return null;

                rc = Construct(archive);
                rc.CacheKey = url;
                rc.IsLocalFileDefinition = !url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && File.Exists(url);
            }
            if (cache)
            {
                DataCache.SetCachedDefinition(url, rc, null);
                rc.FoundInDataCache = true;
            }
            return rc;
        }

    }
}
