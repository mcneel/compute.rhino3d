using System;
using System.Reflection;
using Nancy;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace compute.geometry
{
    class EndPoint
    {
        public EndPoint()
        {
        }
        public EndPoint(string path)
        {
            Path = path;
        }
        Func<NancyContext, Response> _getFunction;
        Func<NancyContext, Response> _postFunction;
        public static EndPoint Create(string path, Func<NancyContext, Response> getFunction, Func<NancyContext, Response> postFunction)
        {
            var endpoint = new EndPoint();
            endpoint.Path = path;
            endpoint._getFunction = getFunction;
            endpoint._postFunction = postFunction;
            return endpoint;
        }
        public static EndPoint CreateGet(string path, Func<NancyContext, Response> getFunction)
        {
            return Create(path, getFunction, null);
        }
        public static EndPoint CreatePost(string path, Func<NancyContext, Response> postFunction)
        {
            return Create(path, null, postFunction);
        }

        public virtual Response Get(NancyContext context)
        {
            if (_getFunction != null)
                return _getFunction(context);
            return null;
        }
        public virtual Response Post(NancyContext context)
        {
            if (_postFunction != null)
                return _postFunction(context);
            return null;
        }

        public string Path { get; protected set; }
    }

    class ListSdkEndPoint : EndPoint
    { 
        public ListSdkEndPoint(string path) : base(path)
        {
        }

        public override Response Get(NancyContext context)
        {
            var sb = new System.Text.StringBuilder("<!DOCTYPE html><html><body>");

            var sb_api = new System.Text.StringBuilder();
            var sb_sdk = new System.Text.StringBuilder();
            sb_api.AppendLine("<p>API<br>");
            var endpoints = EndPointDictionary.GetDictionary().Values;
            int i = 1;
            foreach (var endpoint in endpoints)
            {
                if (!(endpoint.GetType().IsAssignableFrom(typeof(EndPoint))))
                    sb_api.AppendLine((i++).ToString() + $" <a href=\"/{endpoint.Path.ToLowerInvariant()}\">{endpoint.Path}</a><BR>");
            }
            sb_sdk.AppendLine($" <a href=\"/sdk/csharp\">C# SDK</a><BR>");


            sb.Append(sb_sdk);
            sb.Append(sb_api);
            sb.AppendLine("</p></body></html>");
            return sb.ToString();
        }
    }
}


public class TestResolver : DefaultContractResolver
{
    static JsonSerializerSettings _settings;
    public static JsonSerializerSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                _settings = new JsonSerializerSettings { ContractResolver = new TestResolver() };
                // return V6 ON_Objects for now
                var options = new Rhino.FileIO.SerializationOptions();
                options.RhinoVersion = 6;
                options.WriteUserData = true;
                _settings.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
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

        if(property.DeclaringType == typeof(Rhino.Geometry.Point3f) ||
            property.DeclaringType == typeof(Rhino.Geometry.Point2f) ||
            property.DeclaringType == typeof(Rhino.Geometry.Vector2f) ||
            property.DeclaringType == typeof(Rhino.Geometry.Vector3f))
        {
            property.ShouldSerialize = _ =>
            {
                return property.PropertyName == "X" || property.PropertyName == "Y" || property.PropertyName == "Z";
            };
        }

        if(property.DeclaringType == typeof(Rhino.Geometry.MeshFace))
        {
            property.ShouldSerialize = _ =>
            {
                return property.PropertyName != "IsTriangle" && property.PropertyName != "IsQuad";
            };
        }
        return property;
    }
}
