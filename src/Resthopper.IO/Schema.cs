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
        public Dictionary<string, Dictionary<GhPath, List<ResthopperObject>>> Values { get; set; }
        
    }

    public class ResthopperObject
    {
        [JsonProperty(PropertyName = "type")]
        public GHTypeCodes Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }
    }
}
