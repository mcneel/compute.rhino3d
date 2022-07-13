using System;
using System.Collections.Generic;
using System.Reflection;
using Nancy;

namespace compute.geometry
{
    public class InfoEndPointsModule : NancyModule
    {
        public InfoEndPointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Get[""] = _ => HomePage(Context);
            Get["version"] = _ => GetVersion(Context);
            Get["servertime"] = _ => ServerTime(Context);
            Get["sdk/csharp"] = _ => CSharpSdk(Context);
        }

        static Response HomePage(NancyContext ctx)
        {
            return new Nancy.Responses.RedirectResponse("https://www.rhino3d.com/compute");
        }

        static Response GetVersion(NancyContext ctx)
        {
            var values = new Dictionary<string, string>();
            values.Add("rhino", Rhino.RhinoApp.Version.ToString());
            values.Add("compute", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }

        static Response ServerTime(NancyContext ctx)
        {
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(DateTime.UtcNow);
            response.ContentType = "application/json";
            return response;
        }

        static Response CSharpSdk(NancyContext ctx)
        {
            string content = "";
            using (var resourceStream = typeof(InfoEndPointsModule).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
            {
                var stream = new System.IO.StreamReader(resourceStream);
                content = stream.ReadToEnd();
                stream.Close();
            }

            var response = new Response();

            response.Headers.Add("Content-Disposition", "attachment; filename=RhinoCompute.cs");
            response.ContentType = "text/plain";
            response.Contents = stream => {
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    writer.Write(content);
                }
            };
            return response.AsAttachment("RhinoCompute.cs", "text/plain" );
        }
    }
}

