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
                    //GH_Param<IGH_Goo> goo = obj as GH_Param<IGH_Goo>;

                    // SetData
                    int counter = 0;
                    foreach (Resthopper.IO.DataTree<ResthopperObject> tree in input.Values)
                    {
                        string paramname = input.ParamNames[counter++];
                        if (group.NickName == paramname)
                        {
                            switch (code)
                            {
                                case GHTypeCodes.Boolean:
                                    //PopulateParam<GH_Boolean>(goo, tree);
                                    Param_Boolean boolParam = param as Param_Boolean;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Boolean> objectList = new List<GH_Boolean>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Boolean data = JsonConvert.DeserializeObject<GH_Boolean>(restobj.Data);
                                            boolParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Point:
                                    //PopulateParam<GH_Point>(goo, tree);
                                    Param_Point ptParam = param as Param_Point;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Point> objectList = new List<GH_Point>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Point data = JsonConvert.DeserializeObject<GH_Point>(restobj.Data);
                                            ptParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Vector:
                                    //PopulateParam<GH_Vector>(goo, tree);
                                    Param_Vector vectorParam = param as Param_Vector;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Vector> objectList = new List<GH_Vector>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Vector data = JsonConvert.DeserializeObject<GH_Vector>(restobj.Data);
                                            vectorParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Integer:
                                    //PopulateParam<GH_Integer>(goo, tree);
                                    Param_Integer integerParam = param as Param_Integer;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Integer> objectList = new List<GH_Integer>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Integer data = JsonConvert.DeserializeObject<GH_Integer>(restobj.Data);
                                            integerParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Number:
                                    //PopulateParam<GH_Number>(goo, tree);
                                    Param_Number numberParam = param as Param_Number;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Number> objectList = new List<GH_Number>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Number data = JsonConvert.DeserializeObject<GH_Number>(restobj.Data);
                                            numberParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Text:
                                    //PopulateParam<GH_String>(goo, tree);
                                    Param_String stringParam = param as Param_String;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_String> objectList = new List<GH_String>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_String data = JsonConvert.DeserializeObject<GH_String>(restobj.Data);
                                            stringParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Line:
                                    //PopulateParam<GH_Line>(goo, tree);
                                    Param_Line lineParam = param as Param_Line;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Line> objectList = new List<GH_Line>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Line data = JsonConvert.DeserializeObject<GH_Line>(restobj.Data);
                                            lineParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Curve:
                                    //PopulateParam<GH_Curve>(goo, tree);
                                    Param_Curve curveParam = param as Param_Curve;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Curve> objectList = new List<GH_Curve>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Curve data = JsonConvert.DeserializeObject<GH_Curve>(restobj.Data);
                                            curveParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Circle:
                                    //PopulateParam<GH_Circle>(goo, tree);
                                    Param_Circle circleParam = param as Param_Circle;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Circle> objectList = new List<GH_Circle>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Circle data = JsonConvert.DeserializeObject<GH_Circle>(restobj.Data);
                                            circleParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.PLane:
                                    //PopulateParam<GH_Plane>(goo, tree);
                                    Param_Plane planeParam = param as Param_Plane;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Plane> objectList = new List<GH_Plane>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Plane data = JsonConvert.DeserializeObject<GH_Plane>(restobj.Data);
                                            planeParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Rectangle:
                                    //PopulateParam<GH_Rectangle>(goo, tree);
                                    Param_Rectangle rectangleParam = param as Param_Rectangle;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Rectangle> objectList = new List<GH_Rectangle>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Rectangle data = JsonConvert.DeserializeObject<GH_Rectangle>(restobj.Data);
                                            rectangleParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Box:
                                    //PopulateParam<GH_Box>(goo, tree);
                                    Param_Box boxParam = param as Param_Box;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Box> objectList = new List<GH_Box>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Box data = JsonConvert.DeserializeObject<GH_Box>(restobj.Data);
                                            boxParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Surface:
                                    //PopulateParam<GH_Surface>(goo, tree);
                                    Param_Surface surfaceParam = param as Param_Surface;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Surface> objectList = new List<GH_Surface>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Surface data = JsonConvert.DeserializeObject<GH_Surface>(restobj.Data);
                                            surfaceParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Brep:
                                    //PopulateParam<GH_Brep>(goo, tree);
                                    Param_Brep brepParam = param as Param_Brep;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Brep> objectList = new List<GH_Brep>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Brep data = JsonConvert.DeserializeObject<GH_Brep>(restobj.Data);
                                            brepParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Mesh:
                                    //PopulateParam<GH_Mesh>(goo, tree);
                                    Param_Mesh meshParam = param as Param_Mesh;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Mesh> objectList = new List<GH_Mesh>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Mesh data = JsonConvert.DeserializeObject<GH_Mesh>(restobj.Data);
                                            meshParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;

                                case GHTypeCodes.Slider:
                                    //PopulateParam<GH_Number>(goo, tree);
                                    GH_NumberSlider sliderParam = param as GH_NumberSlider;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Number> objectList = new List<GH_Number>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Number data = JsonConvert.DeserializeObject<GH_Number>(restobj.Data);
                                            sliderParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.BooleanToggle:
                                    //PopulateParam<GH_Boolean>(goo, tree);
                                    GH_BooleanToggle toggleParam = param as GH_BooleanToggle;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Boolean> objectList = new List<GH_Boolean>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Boolean data = JsonConvert.DeserializeObject<GH_Boolean>(restobj.Data);
                                            toggleParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
                                case GHTypeCodes.Panel:
                                    //PopulateParam<GH_String>(goo, tree);
                                    GH_Panel panelParam = param as GH_Panel;
                                    foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in tree)
                                    {
                                        GH_Path path = new GH_Path(entree.Key.Path);
                                        List<GH_Panel> objectList = new List<GH_Panel>();
                                        for (int i = 0; i < entree.Value.Count; i++)
                                        {
                                            ResthopperObject restobj = entree.Value[i];
                                            GH_Panel data = JsonConvert.DeserializeObject<GH_Panel>(restobj.Data);
                                            panelParam.AddVolatileData(path, i, data);
                                        }
                                    }
                                    break;
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
                    //OutputTree.ParamName = param.NickName;

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

