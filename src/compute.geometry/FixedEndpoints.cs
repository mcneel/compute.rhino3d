using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.PlugIns;

namespace compute.geometry
{
    public static class FixedEndpoints
    {
        public static Response HomePage(NancyContext ctx)
        {
            return new Nancy.Responses.RedirectResponse("https://www.rhino3d.com/compute");
        }

        public static Response GetVersion(NancyContext ctx)
        {
            var values = new Dictionary<string, string>();
            values.Add("Rhino", Rhino.RhinoApp.Version.ToString());
            values.Add("Compute", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }

        public static Response CSharpSdk(NancyContext ctx)
        {
            string content = "";
            using (var resourceStream = typeof(FixedEndpoints).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
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
        public static Response HammerTime(NancyContext ctx)
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

        public static Response Grasshopper(NancyContext ctx)
        {
            // load grasshopper file
            var archive = new GH_Archive();
            // TODO: stream to string
            var body = ctx.Request.Body.ToString();
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                var xml = reader.ReadToEnd();
                if (!archive.Deserialize_Xml(xml))
                    throw new Exception();
            }

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception();

            //var outputs = new List<Rhino.Geometry.GeometryBase>();
            var outputs = new List<double>();
            foreach (var obj in definition.Objects)
            {
                var param = obj as IGH_Param;
                if (param == null)
                    continue;

                if (param.Sources.Count == 0 || param.Recipients.Count != 0)
                    continue;

                try
                {
                    param.CollectData();
                    param.ComputeData();
                }
                catch (Exception)
                {
                    param.Phase = GH_SolutionPhase.Failed;
                    // TODO: throw something better
                    throw;
                }

                var output = new List<Rhino.Geometry.GeometryBase>();
                var volatileData = param.VolatileData;
                for (int p = 0; p < volatileData.PathCount; p++)
                {
                    foreach (var goo in volatileData.get_Branch(p))
                    {
                        //case GH_Point point: output.Add(new Rhino.Geometry.Point(point.Value)); break;
                        //case GH_Curve curve: output.Add(curve.Value); break;
                        //case GH_Brep brep: output.Add(brep.Value); break;
                        //case GH_Mesh mesh: output.Add(mesh.Value); break;
                        if (goo.GetType() == typeof(GH_Number)) {
                            outputs.Add((goo as GH_Number).Value);
                            break;
                        }

                    }
                }
            }

            if (outputs.Count < 1)
                throw new Exception(); // TODO

            return outputs[0].ToString();
        }
    }
}

