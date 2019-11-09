using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Grasshopper.Kernel.Data;
using Resthopper.IO;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Net;

namespace compute.geometry
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ResthopperComponent
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public bool IsObsolete { get; set; }
        public List<ResthopperComponentParameter> Inputs { get; set; }
        public List<ResthopperComponentParameter> Outputs { get; set; }

        public ResthopperComponent()
        {
            Inputs = new List<ResthopperComponentParameter>();
            Outputs = new List<ResthopperComponentParameter>();
        }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class ResthopperComponentParameter
    {
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Description { get; set; }
        public bool IsOptional { get; set; }
        public string TypeName { get; set; }

        public ResthopperComponentParameter(IGH_Param param)
        {
            Name = param.Name;
            NickName = param.NickName;
            Description = param.InstanceDescription;
            IsOptional = param.Optional;
            TypeName = param.TypeName;
        }
    }

    public class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Get["/grasshopper"] = _ => TranspileGrasshopperAssemblies(Context);
            Post["/grasshopper"] = _ => RunGrasshopper(Context);
            Post["/io"] = _ => GetIoNames(Context);
        }

        static Response TranspileGrasshopperAssemblies(NancyContext ctx)
        {
            var objs = new List<ResthopperComponent>();

            var proxies = Grasshopper.Instances.ComponentServer.ObjectProxies;

            for (int i = 0; i < proxies.Count; i++)
            {
                var rc = new ResthopperComponent();
                rc.Guid = proxies[i].Guid.ToString();
                rc.Name = proxies[i].Desc.Name;
                rc.NickName = proxies[i].Desc.NickName;
                rc.Description = proxies[i].Desc.Description;
                rc.Category = proxies[i].Desc.HasCategory ? proxies[i].Desc.Category : "";
                rc.Subcategory = proxies[i].Desc.HasSubCategory ? proxies[i].Desc.SubCategory : "";
                rc.IsObsolete = proxies[i].Obsolete;

                var obj = proxies[i].CreateInstance() as IGH_Component;

                if (obj != null)
                {
                    var p = obj.Params;

                    p.Input.ForEach(x => rc.Inputs.Add(new ResthopperComponentParameter(x)));
                    p.Output.ForEach(x => rc.Outputs.Add(new ResthopperComponentParameter(x)));
                }

                objs.Add(rc);
            }

            return JsonConvert.SerializeObject(objs);
        }

        static string GetGhxFromPointer(string pointer)
        {
            string grasshopperXml = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pointer);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                grasshopperXml = reader.ReadToEnd();
            }

            return StripBom(grasshopperXml);
        }

        static Response RunGrasshopper(NancyContext ctx)
        {
            // load grasshopper file
            GH_Archive archive = null;

            string json = string.Empty;
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                json = reader.ReadToEnd();
            }

            Schema input = JsonConvert.DeserializeObject<Schema>(json);

            if (input.Algo != null)
            {
                byte[] byteArray = Convert.FromBase64String(input.Algo);
                var byteArchive = new GH_Archive();
                try
                {
                    if (byteArchive.Deserialize_Binary(byteArray))
                        archive = byteArchive;
                }
                catch (Exception) { }

                if( archive==null)
                {
                    var grasshopperXml = StripBom(System.Text.Encoding.UTF8.GetString(byteArray));
                    var xmlArchive = new GH_Archive();
                    if (xmlArchive.Deserialize_Xml(grasshopperXml))
                        archive = xmlArchive;
                }
            }
            else
            {
                string pointer = input.Pointer;
                var grasshopperXml = GetGhxFromPointer(pointer);
                var xmlArchive = new GH_Archive();
                if (xmlArchive.Deserialize_Xml(grasshopperXml))
                    archive = xmlArchive;
            }

            if ( archive == null )
                throw new Exception("Unable to load grasshopper definition");

            using (var definition = new GH_Document())
            {
                if (!archive.ExtractObject(definition, "Definition"))
                    throw new Exception();

                // Set input params
                foreach (var obj in definition.Objects)
                {
                    var group = obj as GH_Group;
                    if (group == null)
                        continue;

                    if (group.NickName.Contains("RH_IN"))
                    {
                        // It is a RestHopper input group!
                        var param = group.Objects()[0];

                        // SetData
                        foreach (Resthopper.IO.DataTree<ResthopperObject> tree in input.Values)
                        {
                            if (group.NickName == tree.ParamName)
                            {
                                {
                                    Param_Boolean boolParam = param as Param_Boolean;
                                    if (boolParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Boolean> objectList = new List<GH_Boolean>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                bool boolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                                                GH_Boolean data = new GH_Boolean(boolean);
                                                boolParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Point ptParam = param as Param_Point;
                                    if (ptParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Point> objectList = new List<GH_Point>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Point3d rPt = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                                                GH_Point data = new GH_Point(rPt);
                                                ptParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Vector vectorParam = param as Param_Vector;
                                    if (vectorParam != null)
                                    { 
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Vector> objectList = new List<GH_Vector>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Vector3d rhVector = JsonConvert.DeserializeObject<Rhino.Geometry.Vector3d>(restobj.Data);
                                                GH_Vector data = new GH_Vector(rhVector);
                                                vectorParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Integer integerParam = param as Param_Integer;
                                    if (integerParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Integer> objectList = new List<GH_Integer>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                int rhinoInt = JsonConvert.DeserializeObject<int>(restobj.Data);
                                                GH_Integer data = new GH_Integer(rhinoInt);
                                                integerParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Number numberParam = param as Param_Number;
                                    if (numberParam != null)
                                    { 
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Number> objectList = new List<GH_Number>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                                                GH_Number data = new GH_Number(rhNumber);
                                                numberParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_String stringParam = param as Param_String;
                                    if (stringParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_String> objectList = new List<GH_String>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                string rhString = restobj.Data;
                                                GH_String data = new GH_String(rhString);
                                                stringParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Line lineParam = param as Param_Line;
                                    if (lineParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Line> objectList = new List<GH_Line>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Line rhLine = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                                                GH_Line data = new GH_Line(rhLine);
                                                lineParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Curve curveParam = param as Param_Curve;
                                    if (curveParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Curve> objectList = new List<GH_Curve>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                GH_Curve ghCurve;
                                                try
                                                {
                                                    Rhino.Geometry.Polyline data = JsonConvert.DeserializeObject<Rhino.Geometry.Polyline>(restobj.Data);
                                                    Rhino.Geometry.Curve c = new Rhino.Geometry.PolylineCurve(data);
                                                    ghCurve = new GH_Curve(c);
                                                }
                                                catch
                                                {
                                                    Rhino.Geometry.NurbsCurve data = JsonConvert.DeserializeObject<Rhino.Geometry.NurbsCurve>(restobj.Data);
                                                    Rhino.Geometry.Curve c = new Rhino.Geometry.NurbsCurve(data);
                                                    ghCurve = new GH_Curve(c);
                                                }
                                                curveParam.AddVolatileData(path, i, ghCurve);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Circle circleParam = param as Param_Circle;
                                    if (circleParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Circle> objectList = new List<GH_Circle>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Circle rhCircle = JsonConvert.DeserializeObject<Rhino.Geometry.Circle>(restobj.Data);
                                                GH_Circle data = new GH_Circle(rhCircle);
                                                circleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }


                                    Param_Plane planeParam = param as Param_Plane;
                                    if (planeParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Plane> objectList = new List<GH_Plane>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Plane rhPlane = JsonConvert.DeserializeObject<Rhino.Geometry.Plane>(restobj.Data);
                                                GH_Plane data = new GH_Plane(rhPlane);
                                                planeParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    Param_Rectangle rectangleParam = param as Param_Rectangle;
                                    if (rectangleParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Rectangle> objectList = new List<GH_Rectangle>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Rectangle3d rhRectangle = JsonConvert.DeserializeObject<Rhino.Geometry.Rectangle3d>(restobj.Data);
                                                GH_Rectangle data = new GH_Rectangle(rhRectangle);
                                                rectangleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    Param_Box boxParam = param as Param_Box;
                                    if (boxParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Box> objectList = new List<GH_Box>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Box rhBox = JsonConvert.DeserializeObject<Rhino.Geometry.Box>(restobj.Data);
                                                GH_Box data = new GH_Box(rhBox);
                                                boxParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    Param_Surface surfaceParam = param as Param_Surface;
                                    if (surfaceParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Surface> objectList = new List<GH_Surface>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Surface rhSurface = JsonConvert.DeserializeObject<Rhino.Geometry.Surface>(restobj.Data);
                                                GH_Surface data = new GH_Surface(rhSurface);
                                                surfaceParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    Param_Brep brepParam = param as Param_Brep;
                                    if (brepParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Brep> objectList = new List<GH_Brep>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Brep rhBrep = JsonConvert.DeserializeObject<Rhino.Geometry.Brep>(restobj.Data);
                                                GH_Brep data = new GH_Brep(rhBrep);
                                                brepParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    Param_Mesh meshParam = param as Param_Mesh;
                                    if (meshParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Mesh> objectList = new List<GH_Mesh>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Mesh rhMesh = JsonConvert.DeserializeObject<Rhino.Geometry.Mesh>(restobj.Data);
                                                GH_Mesh data = new GH_Mesh(rhMesh);
                                                meshParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    GH_NumberSlider sliderParam = param as GH_NumberSlider;
                                    if (sliderParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Number> objectList = new List<GH_Number>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                                                GH_Number data = new GH_Number(rhNumber);
                                                sliderParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    GH_BooleanToggle toggleParam = param as GH_BooleanToggle;
                                    if (toggleParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Boolean> objectList = new List<GH_Boolean>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                bool rhBoolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                                                GH_Boolean data = new GH_Boolean(rhBoolean);
                                                toggleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }

                                    GH_Panel panelParam = param as GH_Panel;
                                    if (panelParam != null)
                                    {
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Panel> objectList = new List<GH_Panel>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                string rhString = JsonConvert.DeserializeObject<string>(restobj.Data);
                                                GH_String data = new GH_String(rhString);
                                                panelParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                        }


                    }
                }

                Schema OutputSchema = new Schema();
                OutputSchema.Algo = Utils.Base64Encode(string.Empty);

                // Parse output params
                foreach (var obj in definition.Objects)
                {
                    var group = obj as GH_Group;
                    if (group == null)
                        continue;

                    if (group.NickName.Contains("RH_OUT"))
                    {
                        // It is a RestHopper output group!
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
                        OutputTree.ParamName = group.NickName;

                        var volatileData = param.VolatileData;
                        for (int p = 0; p < volatileData.PathCount; p++)
                        {
                            List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                            foreach (var goo in volatileData.get_Branch(p))
                            {
                                if (goo == null)
                                    continue;
                                else if (goo.GetType() == typeof(GH_Boolean))
                                {
                                    GH_Boolean ghValue = goo as GH_Boolean;
                                    bool rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<bool>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Point))
                                {
                                    GH_Point ghValue = goo as GH_Point;
                                    Point3d rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Point3d>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Vector))
                                {
                                    GH_Vector ghValue = goo as GH_Vector;
                                    Vector3d rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Vector3d>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Integer))
                                {
                                    GH_Integer ghValue = goo as GH_Integer;
                                    int rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<int>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Number))
                                {
                                    GH_Number ghValue = goo as GH_Number;
                                    double rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<double>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_String))
                                {
                                    GH_String ghValue = goo as GH_String;
                                    string rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<string>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Line))
                                {
                                    GH_Line ghValue = goo as GH_Line;
                                    Line rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Line>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Curve))
                                {
                                    GH_Curve ghValue = goo as GH_Curve;
                                    Curve rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Curve>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Circle))
                                {
                                    GH_Circle ghValue = goo as GH_Circle;
                                    Circle rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Circle>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Plane))
                                {
                                    GH_Plane ghValue = goo as GH_Plane;
                                    Plane rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Plane>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Rectangle))
                                {
                                    GH_Rectangle ghValue = goo as GH_Rectangle;
                                    Rectangle3d rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Rectangle3d>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Box))
                                {
                                    GH_Box ghValue = goo as GH_Box;
                                    Box rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Box>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Surface))
                                {
                                    GH_Surface ghValue = goo as GH_Surface;
                                    Brep rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Brep))
                                {
                                    GH_Brep ghValue = goo as GH_Brep;
                                    Brep rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                else if (goo.GetType() == typeof(GH_Mesh))
                                {
                                    GH_Mesh ghValue = goo as GH_Mesh;
                                    Mesh rhValue = ghValue.Value;
                                    ResthopperObjectList.Add(GetResthopperObject<Mesh>(rhValue));
                                }
                            }

                            GhPath path = new GhPath(new int[] { p });
                            OutputTree.Add(path.ToString(), ResthopperObjectList);
                        }

                        OutputSchema.Values.Add(OutputTree);
                    }
                }


                if (OutputSchema.Values.Count < 1)
                    throw new System.Exceptions.PayAttentionException("Looks like you've missed something..."); // TODO

                string returnJson = JsonConvert.SerializeObject(OutputSchema, GeometryResolver.Settings);
                return returnJson;
            }
        }

        static Response GetIoNames(NancyContext ctx)
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

            IoQuerySchema input = JsonConvert.DeserializeObject<IoQuerySchema>(json);
            string pointer = input.RequestedFile;
            string grasshopperXml = GetGhxFromPointer(pointer);

            if (!archive.Deserialize_Xml(grasshopperXml))
                throw new Exception();

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
            {
                throw new Exception();
            }

            // Parse input and output names
            List<string> InputNames = new List<string>();
            List<string> OutputNames = new List<string>();
            foreach (var obj in definition.Objects)
            {
                var group = obj as GH_Group;
                if (group == null) continue;

                if (group.NickName.Contains("RH_IN"))
                {
                    InputNames.Add(group.NickName);
                }
                else if (group.NickName.Contains("RH_OUT"))
                {
                    OutputNames.Add(group.NickName);
                }
            }

            IoResponseSchema response = new IoResponseSchema();
            response.InputNames = InputNames;
            response.OutputNames = OutputNames;

            string jsonResponse = JsonConvert.SerializeObject(response);
            return jsonResponse;
        }

        public static ResthopperObject GetResthopperPoint(GH_Point goo)
        {
            var pt = goo.Value;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = pt.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(pt, GeometryResolver.Settings);
            return rhObj;

        }
        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v, GeometryResolver.Settings);
            return rhObj;
        }
        public static void PopulateParam<DataType>(GH_Param<IGH_Goo> Param, Resthopper.IO.DataTree<ResthopperObject> tree)
        {

            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                List<DataType> objectList = new List<DataType>();
                for (int i = 0; i < entree.Value.Count; i++)
                {
                    ResthopperObject obj = entree.Value[i];
                    DataType data = JsonConvert.DeserializeObject<DataType>(obj.Data);
                    Param.AddVolatileData(path, i, data);
                }

            }

        }

        // strip bom from string -- [239, 187, 191] in byte array == (char)65279
        // https://stackoverflow.com/a/54894929/1902446
        static string StripBom(string str)
        {
            if (!string.IsNullOrEmpty(str) && str[0] == (char)65279)
                str = str.Substring(1);

            return str;
        }
    }
}

namespace System.Exceptions
{
    public class PayAttentionException : Exception
    {
        public PayAttentionException(string m) : base(m)
        {

        }

    }
}
