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
        public static GrasshopperDefinition FromBase64String(string data, bool cache)
        {
            var archive = ArchiveFromBase64String(data);
            if (archive == null)
                return null;

            var rc = Construct(archive);
            if (rc != null)
            {
                rc.CacheKey = DataCache.CreateCacheKey(data);
                if (cache)
                {
                    DataCache.SetCachedDefinition(rc.CacheKey, rc, data);
                    rc.FoundInDataCache = true;
                }
            }
            return rc;
        }
    }
}
