using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.PlugIns;
using Newtonsoft.Json;
using Grasshopper;
using Grasshopper.Kernel.Data;

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
            //
            //var body = input.Algo;

            string json = string.Empty;
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                json = reader.ReadToEnd();

            }

            GrasshopperInput input = Newtonsoft.Json.JsonConvert.DeserializeObject<GrasshopperInput>(json);

            byte[] byteArray = Convert.FromBase64String(input.Algo);
            string grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray);

            if (!archive.Deserialize_Xml(grasshopperXml))
                throw new Exception();

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception();

            foreach (var obj in definition.Objects) {
                var param = obj as IGH_Param;

                if (param == null) continue;
                
                //this is an input!
                if(param.Sources.Count == 0 && param.Recipients.Count != 0) {
                    string nick = param.NickName;
                    if (input.Values.ContainsKey(nick)) {
                        var val = input.Values[nick];
                        
                        IGH_Structure data = param.VolatileData;

                        GH_Number num = new GH_Number(Convert.ToDouble(val.ToString()));

                        param.AddVolatileData(new GH_Path(0), 0, num);
                    }
                }
            }
                //var outputs = new List<Rhino.Geometry.GeometryBase>();
                //var outputs = new List<double>();
            GrasshopperOutput outputs = new GrasshopperOutput();
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
                        if (goo == null) continue;
                        //case GH_Point point: output.Add(new Rhino.Geometry.Point(point.Value)); break;
                        //case GH_Curve curve: output.Add(curve.Value); break;
                        //case GH_Brep brep: output.Add(brep.Value); break;
                        //case GH_Mesh mesh: output.Add(mesh.Value); break;
                        if (goo.GetType() == typeof(GH_Number)) {

                            GrasshopperOutputItem item = new GrasshopperOutputItem();
                            item.Data = (goo as GH_Number).Value.ToString();
                            item.TypeHint = "number";
                            outputs.Items.Add(item);
                            //break;
                        } else if(goo.GetType() == typeof(GH_Mesh)) {
                            var rhinoMesh = (goo as GH_Mesh).Value;
                            string jsonMesh = JsonConvert.SerializeObject(rhinoMesh);
                            GrasshopperOutputItem item = new GrasshopperOutputItem();
                            item.Data = jsonMesh;
                            item.TypeHint = "mesh";
                            outputs.Items.Add(item);
                            //break;
                        } else if(goo.GetType() == typeof(GH_Circle)) {
                            var rhinoCircles = (goo as GH_Circle).Value;
                            string jsonCircle = JsonConvert.SerializeObject(rhinoCircles);
                            GrasshopperOutputItem item = new GrasshopperOutputItem();
                            item.Data = jsonCircle;
                            item.TypeHint = "circle";
                            outputs.Items.Add(item);
                            //break;
                        }
                    }
                }
            }

            if (outputs.Items.Count < 1)
                throw new System.Exceptions.DontFuckUpException("Don't mess up, asshole"); // TODO

            string returnJson = JsonConvert.SerializeObject(outputs);
            return returnJson;
        }
        
    }
}

namespace System.Exceptions
{
    public class DontFuckUpException : Exception
    {
        public DontFuckUpException(string m) : base(m) {

        }

    }
}

