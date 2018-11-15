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
using Resthopper.IO;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;

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

            //GrasshopperInput input = Newtonsoft.Json.JsonConvert.DeserializeObject<GrasshopperInput>(json);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new DictionaryAsArrayResolver();
            Schema input = JsonConvert.DeserializeObject<Schema>(json, settings);

            byte[] byteArray = Convert.FromBase64String(input.Algo);
            string grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray);

            if (!archive.Deserialize_Xml(grasshopperXml))
                throw new Exception();

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception();

            // Set input params
            foreach (var obj in definition.Objects)
            {
                var group = obj as GH_Group;
                if (group == null) continue;

                if (group.NickName.Contains("RH_IN"))
                {
                    // It is a RestHopper input group!
                    GHTypeCodes code = (GHTypeCodes)Int32.Parse(group.NickName.Split(':')[1]);
                    var param = group.Objects()[0];
                    GH_Param<IGH_Goo> goo = obj as GH_Param<IGH_Goo>;

                    // SetData
                    foreach (Resthopper.IO.DataTree<ResthopperObject> tree in input.Values)
                    {
                        if (param.NickName == tree.ParamName)
                        {
                            switch (code)
                            {
                                case GHTypeCodes.Boolean: PopulateParam<GH_Boolean>(goo, tree); break;
                                case GHTypeCodes.Point: PopulateParam<GH_Point>(goo, tree); break;
                                case GHTypeCodes.Vector: PopulateParam<GH_Vector>(goo, tree); break;
                                case GHTypeCodes.Integer: PopulateParam<GH_Integer>(goo, tree); break;
                                case GHTypeCodes.Number: PopulateParam<GH_Number>(goo, tree); break;
                                case GHTypeCodes.Text: PopulateParam<GH_String>(goo, tree); break;
                                case GHTypeCodes.Line: PopulateParam<GH_Line>(goo, tree); break;
                                case GHTypeCodes.Curve: PopulateParam<GH_Curve>(goo, tree); break;
                                case GHTypeCodes.Circle: PopulateParam<GH_Circle>(goo, tree); break;
                                case GHTypeCodes.PLane: PopulateParam<GH_Plane>(goo, tree); break;
                                case GHTypeCodes.Rectangle: PopulateParam<GH_Rectangle>(goo, tree); break;
                                case GHTypeCodes.Box: PopulateParam<GH_Box>(goo, tree); break;
                                case GHTypeCodes.Surface: PopulateParam<GH_Surface>(goo, tree); break;
                                case GHTypeCodes.Brep: PopulateParam<GH_Brep>(goo, tree); break;
                                case GHTypeCodes.Mesh: PopulateParam<GH_Mesh>(goo, tree); break;

                                case GHTypeCodes.Slider: PopulateParam<GH_Number>(goo, tree); break;
                                case GHTypeCodes.BooleanToggle: PopulateParam<GH_Boolean>(goo, tree); break;
                                case GHTypeCodes.Panel: PopulateParam<GH_String>(goo, tree); break;
                            }
                        }
                    }
                    
                    
                }
            }

            Schema OutputSchema = new Schema();
            OutputSchema.Algo = Base64Encode(string.Empty);

            // Parse output params
            foreach (var obj in definition.Objects)
            {
                var group = obj as GH_Group;
                if (group == null) continue;

                if (group.NickName.Contains("RH_OUT"))
                {
                    // It is a RestHopper output group!
                    GHTypeCodes code = (GHTypeCodes)Int32.Parse(group.NickName.Split(':')[1]);
                    var param = group.Objects()[0] as IGH_Param;
                    if (param == null)
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

                    // Get data
                    Resthopper.IO.DataTree<ResthopperObject> OutputTree = new Resthopper.IO.DataTree<ResthopperObject>();
                    OutputTree.ParamName = param.NickName;

                    var volatileData = param.VolatileData;
                    for (int p = 0; p < volatileData.PathCount; p++)
                    {
                        List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                        foreach (var goo in volatileData.get_Branch(p))
                        {
                            if (goo == null) continue;
                            else if (goo.GetType() == typeof(GH_Boolean)) { ResthopperObjectList.Add(GetResthopperObject<GH_Boolean>(goo)); }
                            else if (goo.GetType() == typeof(GH_Point)) { ResthopperObjectList.Add(GetResthopperObject<GH_Point>(goo)); }
                            else if (goo.GetType() == typeof(GH_Vector)) { ResthopperObjectList.Add(GetResthopperObject<GH_Vector>(goo)); }
                            else if (goo.GetType() == typeof(GH_Integer)) { ResthopperObjectList.Add(GetResthopperObject<GH_Integer>(goo)); }
                            else if (goo.GetType() == typeof(GH_Number)) { ResthopperObjectList.Add(GetResthopperObject<GH_Number>(goo)); }
                            else if (goo.GetType() == typeof(GH_String)) { ResthopperObjectList.Add(GetResthopperObject<GH_String>(goo)); }
                            else if (goo.GetType() == typeof(GH_Line)) { ResthopperObjectList.Add(GetResthopperObject<GH_Line>(goo)); }
                            else if (goo.GetType() == typeof(GH_Curve)) { ResthopperObjectList.Add(GetResthopperObject<GH_Curve>(goo)); }
                            else if (goo.GetType() == typeof(GH_Circle)) { ResthopperObjectList.Add(GetResthopperObject<GH_Circle>(goo)); }
                            else if (goo.GetType() == typeof(GH_Plane)) { ResthopperObjectList.Add(GetResthopperObject<GH_Plane>(goo)); }
                            else if (goo.GetType() == typeof(GH_Rectangle)) { ResthopperObjectList.Add(GetResthopperObject<GH_Rectangle>(goo)); }
                            else if (goo.GetType() == typeof(GH_Box)) { ResthopperObjectList.Add(GetResthopperObject<GH_Box>(goo)); }
                            else if (goo.GetType() == typeof(GH_Surface)) { ResthopperObjectList.Add(GetResthopperObject<GH_Surface>(goo)); }
                            else if (goo.GetType() == typeof(GH_Brep)) { ResthopperObjectList.Add(GetResthopperObject<GH_Brep>(goo)); }
                            else if (goo.GetType() == typeof(GH_Mesh)) { ResthopperObjectList.Add(GetResthopperObject<GH_Mesh>(goo)); }
                        }

                        GhPath path = new GhPath(new int[] { p });
                        OutputTree.Add(path, ResthopperObjectList);
                    }

                    OutputSchema.Values.Add(OutputTree);
                }
            }


            if (OutputSchema.Values.Count < 1)
                throw new System.Exceptions.DontFuckUpException("Don't mess up, asshole"); // TODO

            string returnJson = JsonConvert.SerializeObject(OutputSchema, settings);
            return returnJson;
        }
        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;
            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType();
            rhObj.Data = JsonConvert.SerializeObject(v);
            return rhObj;
        }
        public static void PopulateParam<DataType>(GH_Param<IGH_Goo> Param, Resthopper.IO.DataTree<ResthopperObject> tree)
        { 

            foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(entree.Key.Path);
                List<DataType> objectList = new List<DataType>();
                for (int i = 0; i < entree.Value.Count; i ++)
                {
                    ResthopperObject obj = entree.Value[i];
                    DataType data = JsonConvert.DeserializeObject<DataType>(obj.Data);
                    Param.AddVolatileData(path, i, data);
                }
                
            }

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
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

