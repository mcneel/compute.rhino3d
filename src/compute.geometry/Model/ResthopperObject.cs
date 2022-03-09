using System;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class ResthopperObject : IEquatable<ResthopperObject>
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonIgnore]
        public object ResolvedData { get; set; }

        [JsonConstructor]
        public ResthopperObject()
        {
        }

        public ResthopperObject(object obj)
        {
#if COMPUTE_CORE
            Data = JsonConvert.SerializeObject(obj, compute.geometry.GeometryResolver.Settings);
#else
            Data = JsonConvert.SerializeObject(obj);//, compute.geometry.GeometryResolver.Settings);
#endif
            Type = obj.GetType().FullName;
        }

        public bool Equals(ResthopperObject other)
        {
            return string.Equals(Type, other.Type) && string.Equals(Data, other.Data);
        }
    }
}
