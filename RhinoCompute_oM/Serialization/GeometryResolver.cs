using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    public class GeometryResolver : DefaultContractResolver
    {
        static JsonSerializerSettings _settings;
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new JsonSerializerSettings { 
                        ContractResolver = new GeometryResolver(), 
                        TypeNameHandling = TypeNameHandling.Auto, 
                        NullValueHandling = NullValueHandling.Ignore,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                    };
                    // return V6 ON_Objects for now
                    var options = new Rhino.FileIO.SerializationOptions();
                    options.RhinoVersion = 6;
                    options.WriteUserData = true;
                    _settings.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
                    _settings.Converters.Add(new ArchivableDictionaryResolver());
                }
                return _settings;
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == typeof(Rhino.Geometry.Circle))
            {
                property.ShouldSerialize = _ =>
                {
                    return property.PropertyName != "IsValid" && property.PropertyName != "BoundingBox" && property.PropertyName != "Diameter" && property.PropertyName != "Circumference";
                };

            }
            if (property.DeclaringType == typeof(Rhino.Geometry.Plane))
            {
                property.ShouldSerialize = _ =>
                {
                    return property.PropertyName != "IsValid" && property.PropertyName != "OriginX" && property.PropertyName != "OriginY" && property.PropertyName != "OriginZ";
                };
            }

            if (property.DeclaringType == typeof(Rhino.Geometry.Point3f) ||
                property.DeclaringType == typeof(Rhino.Geometry.Point2f) ||
                property.DeclaringType == typeof(Rhino.Geometry.Vector2f) ||
                property.DeclaringType == typeof(Rhino.Geometry.Vector3f))
            {
                property.ShouldSerialize = _ =>
                {
                    return property.PropertyName == "X" || property.PropertyName == "Y" || property.PropertyName == "Z";
                };
            }

            if (property.DeclaringType == typeof(Rhino.Geometry.Line))
            {
                property.ShouldSerialize = _ =>
                {
                    return property.PropertyName == "From" || property.PropertyName == "To";
                };
            }

            if (property.DeclaringType == typeof(Rhino.Geometry.MeshFace))
            {
                property.ShouldSerialize = _ =>
                {
                    return property.PropertyName != "IsTriangle" && property.PropertyName != "IsQuad";
                };
            }
            return property;
        }
    }
}
