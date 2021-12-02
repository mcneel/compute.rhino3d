using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace rhino.compute.Logging
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
