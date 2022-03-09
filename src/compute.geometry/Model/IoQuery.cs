using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class IoQuery
    {
        [JsonProperty(PropertyName = "requestedFile")]
        public string RequestedFile { get; set; }
    }
}
