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
        public static GrasshopperDefinition FromBase64String(string base64string, bool cacheToDisk)
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
