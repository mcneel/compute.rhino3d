using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compute.geometry
{
        public class GrasshopperInput
        {
            [JsonProperty(PropertyName = "algo")]
            public string Algo { get; set; }

            [JsonProperty(PropertyName = "values")]
            public Dictionary<string, object> Values { get; set; }
        }

        public class GrasshopperOutput
        {
            public GrasshopperOutput()
            {
                this.Items = new List<GrasshopperOutputItem>();
            }

            [JsonProperty(PropertyName = "items")]
            public List<GrasshopperOutputItem> Items { get; set; }
        }

        public class GrasshopperOutputItem
        {
            [JsonProperty(PropertyName = "type")]
            public string TypeHint { get; set; }
            [JsonProperty(PropertyName = "data")]
            public string Data { get; set; }
        }
   
}
