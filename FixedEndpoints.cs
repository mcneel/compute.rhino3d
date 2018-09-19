using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace RhinoCommon.Rest
{
    public static class FixedEndpoints
    {
        public static Response GetVersion(NancyContext ctx)
        {
            var values = new Dictionary<string, string>();
            values.Add("Rhino", Rhino.RhinoApp.Version.ToString());
            values.Add("Compute", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }
    }
}
