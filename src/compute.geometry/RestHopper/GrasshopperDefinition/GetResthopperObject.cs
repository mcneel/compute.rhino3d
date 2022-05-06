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
using BH.Engine.RemoteCompute;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        static ResthopperObject GetResthopperObject<T>(object gooValue)
        {
            var castedGoo = (T)gooValue;
            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = gooValue.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(castedGoo, GeometryResolver.JsonSerializerSettings);
            return rhObj;
        }

        static ResthopperObject GetResthopperObject(object goo)
        {
            ResthopperObject result = new ResthopperObject();

            Type gooType = goo.GetType();
            result.Type = gooType.FullName;

            dynamic gooValue = null;

            try
            {
                gooValue = (goo as dynamic).Value;
            }
            catch
            {
                return result;
            }

            Type equivalentRhinoType = gooType.EquivalentRhinoType();

            if (equivalentRhinoType != null)
            {
                // Serialize data with GeometryResolver settings
                dynamic castedGooValue = BH.Engine.RemoteCompute.Modify.CastTo(gooValue, equivalentRhinoType);

                result.Data = JsonConvert.SerializeObject(castedGooValue, GeometryResolver.JsonSerializerSettings);
            }
            else
            {
                // Serialize data with TypeNameHandling auto.
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                result.Data = JsonConvert.SerializeObject(gooValue, settings);
            }

            return result;
        }
    }
}
