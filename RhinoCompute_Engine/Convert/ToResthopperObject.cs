using System;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Newtonsoft.Json;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static ResthopperObject ToResthopperObject<T>(object gooValue)
        {
            var castedGoo = (T)gooValue;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = gooValue.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(castedGoo, GeometryResolver.JsonSerializerSettings);

            return rhObj;
        }

        public static ResthopperObject ToResthopperObject(object goo)
        {
            ResthopperObject result = new ResthopperObject();

            Type gooType = goo.GetType();
            result.Type = gooType.FullName;

            if (result.Type.StartsWith("BH.UI"))
                result.Type = (goo as dynamic).Value.GetType().AssemblyQualifiedName;

            dynamic gooValue = null;

            try
            {
                gooValue = (goo as dynamic).Value;
            }
            catch
            {
                return result;
            }

            Type equivalentRhinoType = gooType.GHToRhinoType(false);

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
