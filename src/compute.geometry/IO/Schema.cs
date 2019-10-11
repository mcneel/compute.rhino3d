using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class Schema
    {

        public Schema() {
            this.Values = new List<DataTree<ResthopperObject>>();
        }

        [JsonProperty(PropertyName = "algo")]
        public string Algo { get; set; }

        [JsonProperty(PropertyName = "pointer")]
        public string Pointer { get; set; }

        [JsonProperty(PropertyName = "values")]

        public List<DataTree<ResthopperObject>> Values { get; set; }

        //public Dictionary<string, Dictionary<GhPath, List<ResthopperObject>>> Values { get; set; }

    }

    public class IoQuerySchema
    {
        [JsonProperty(PropertyName = "requestedFile")]
        public string RequestedFile { get; set; }
    }

    public class IoResponseSchema
    {
        public List<string> InputNames { get; set; }
        public List<string> OutputNames { get; set; }
    }

    public class ResthopperObject
    {
        [JsonProperty(PropertyName = "type")]
        //public Type Type { get; set; }
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonConstructor]
        public ResthopperObject()
        {

        }

        public ResthopperObject(object obj)
        {
            this.Data = JsonConvert.SerializeObject(obj, compute.geometry.GeometryResolver.Settings);
            this.Type = obj.GetType().FullName;
        }
    }
}
