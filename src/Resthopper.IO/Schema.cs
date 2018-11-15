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
        
        [JsonProperty(PropertyName = "algo")]
        public string Algo { get; set; }

        [JsonProperty(PropertyName = "values")]

        public List<DataTree<ResthopperObject>> Values { get; set; }

        //public Dictionary<string, Dictionary<GhPath, List<ResthopperObject>>> Values { get; set; }

    }

    public class ResthopperObject
    {
        [JsonProperty(PropertyName = "type")]
        public Type Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonConstructor]
        public ResthopperObject()
        {

        }

        public ResthopperObject(object obj)
        {
            this.Data = JsonConvert.SerializeObject(obj);
            this.Type = obj.GetType();
        }

        public object ExtractData()
        {
            return JsonConvert.DeserializeObject<object>(this.Data);
        }
    }
}
