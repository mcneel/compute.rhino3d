using System;
using System.Collections.Generic;
using System.Reflection;
using Nancy;

namespace compute.geometry
{
    public class FixedEndPointsModule : NancyModule
    {
        public FixedEndPointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Get[""] = _ => HomePage(Context);
            Get["/healthcheck"] = _ => "healthy";
            Get["version"] = _ => GetVersion(Context);
            Get["servertime"] = _ => ServerTime(Context);
            Get["sdk/csharp"] = _ => CSharpSdk(Context);
            Post["hammertime"] = _ => HammerTime(Context);
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
            string git_sha = null; // appveyor will replace this
            values.Add("git_sha", git_sha);
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
            using (var resourceStream = typeof(FixedEndPointsModule).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
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

        /// <summary>
        /// Do a heavy mesh boolean (for testing things like auto-scaling)
        /// </summary>
        static Response HammerTime(NancyContext ctx)
        {
            // protect from abuse - "X-Commpute-Secret" header must match COMPUTE_SECRET env var
            var secret = Environment.GetEnvironmentVariable("COMPUTE_SECRET");
            if (string.IsNullOrEmpty(secret))
                return HttpStatusCode.NotFound;
            var client_secrets = new List<string>(ctx.Request.Headers["X-Compute-Secret"]);
            if (client_secrets.Count != 1 || client_secrets[0] != secret)
                return HttpStatusCode.NotFound;

            Console.WriteLine("It's hammer time!", null);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var pt = Rhino.Geometry.Point3d.Origin;
            var vec = Rhino.Geometry.Vector3d.ZAxis;
            vec.Unitize();
            var sp1 = new Rhino.Geometry.Sphere(pt, 12);
            var msp1 = Rhino.Geometry.Mesh.CreateFromSphere(sp1, 1000, 1000);
            var msp2 = msp1.DuplicateMesh();
            msp2.Translate(new Rhino.Geometry.Vector3d(10, 10, 10));
            var msp3 = Rhino.Geometry.Mesh.CreateBooleanIntersection(
                new Rhino.Geometry.Mesh[] { msp1 },
                new Rhino.Geometry.Mesh[] { msp2 }
                );

            watch.Stop();
            Console.WriteLine($"The party lasted for {watch.Elapsed.TotalSeconds} seconds!", null);

            var values = new Dictionary<string, double>() {
                { "answer", msp3[0].Volume() },
                { "elapsed_time", watch.Elapsed.TotalMilliseconds }
            };
            var response = (Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }
    }
}

