using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;

namespace compute.geometry
{
    class GrasshopperDefinition
    {
        static Dictionary<string, FileSystemWatcher> _filewatchers;
        static HashSet<string> _watchedFiles = new HashSet<string>();
        static uint _watchedFileRuntimeSerialNumber = 1;
        public static uint WatchedFileRuntimeSerialNumber
        {
            get { return _watchedFileRuntimeSerialNumber; }
        }
        static void RegisterFileWatcher(string path)
        {
            if (_filewatchers == null)
            {
                _filewatchers = new Dictionary<string, FileSystemWatcher>();
            }
            if (!File.Exists(path))
                return;

            path = Path.GetFullPath(path);
            if (_watchedFiles.Contains(path.ToLowerInvariant()))
                return;

            _watchedFiles.Add(path.ToLowerInvariant());
            string directory = Path.GetDirectoryName(path);
            if (_filewatchers.ContainsKey(directory) || !Directory.Exists(directory))
                return;

            var fsw = new FileSystemWatcher(directory);
            fsw.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.Security;
            fsw.Changed += Fsw_Changed;
            fsw.EnableRaisingEvents = true;
            _filewatchers[directory] = fsw;
        }

        private static void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath.ToLowerInvariant();
            if (_watchedFiles.Contains(path))
                _watchedFileRuntimeSerialNumber++;
        }

        static void LogDebug(string message) { Serilog.Log.Debug(message); }
        static void LogError(string messge)  { Serilog.Log.Error(messge); }

        public static GrasshopperDefinition FromUrl(string url, bool cache)
        {
            GrasshopperDefinition rc = DataCache.GetCachedDefinition(url);

            if (rc != null)
            {
                LogDebug("Using cached definition");
                return rc;
            }

            if (Guid.TryParse(url, out Guid componentId))
            {
                rc = Construct(componentId);
            }
            else
            {
                var archive = ArchiveFromUrl(url);
                if (archive == null)
                    return null;

                rc = Construct(archive);
                rc.CacheKey = url;
                rc.IsLocalFileDefinition = !url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && File.Exists(url);
            }
            if (cache)
            {
                DataCache.SetCachedDefinition(url, rc);
                rc.InDataCache = true;
            }
            return rc;
        }

        public static GrasshopperDefinition FromBase64String(string data)
        {
            var archive = ArchiveFromBase64String(data);
            if (archive == null)
                return null;

            var rc = Construct(archive);
            if (rc!=null)
            {
                rc.CacheKey = CreateMD5(data);
            }
            return rc;
        }

        static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private static GrasshopperDefinition Construct(Guid componentId)
        {
            var component = Grasshopper.Instances.ComponentServer.EmitObject(componentId) as GH_Component;
            if (component==null)
                return null;

            var definition = new GH_Document();
            definition.AddObject(component, false);

            // raise DocumentServer.DocumentAdded event (used by some plug-ins)
            Grasshopper.Instances.DocumentServer.AddDocument(definition);

            GrasshopperDefinition rc = new GrasshopperDefinition(definition);
            rc._singularComponent = component;
            foreach(var input in component.Params.Input)
            {
                rc._input[input.NickName] = new InputGroup(input);
            }
            foreach(var output in component.Params.Output)
            {
                rc._output[output.NickName] = output;
            }
            return rc;
        }

        private static GrasshopperDefinition Construct(GH_Archive archive)
        {
            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception("Unable to extract definition from archive");

            // raise DocumentServer.DocumentAdded event (used by some plug-ins)
            Grasshopper.Instances.DocumentServer.AddDocument(definition);

            GrasshopperDefinition rc = new GrasshopperDefinition(definition);
            foreach( var obj in definition.Objects)
            {
                IGH_ContextualParameter contextualParam = obj as IGH_ContextualParameter;
                if (contextualParam != null)
                {
                    IGH_Param param = obj as IGH_Param;
                    if (param != null)
                    {
                        rc._input[param.NickName] = new InputGroup(param);
                    }
                    continue;
                }

                var group = obj as GH_Group;
                if (group == null)
                    continue;

                string nickname = group.NickName;
                var groupObjects = group.Objects();
                if ( nickname.Contains("RH_IN") && groupObjects.Count>0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null)
                    {
                        rc._input[nickname] = new InputGroup(param);
                    }
                }

                if (nickname.Contains("RH_OUT") && groupObjects.Count > 0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null)
                    {
                        rc._output[nickname] = param;
                    }
                }
            }
            return rc;
        }

        private GrasshopperDefinition(GH_Document definition)
        {
            Definition = definition;
            FileRuntimeCacheSerialNumber = _watchedFileRuntimeSerialNumber;
        }

        public GH_Document Definition { get; }
        public bool InDataCache { get; set; }
        public bool HasErrors { get; private set; } // default: false
        public bool IsLocalFileDefinition { get; set; } // default: false
        public uint FileRuntimeCacheSerialNumber { get; private set; }
        public string CacheKey { get; set; }
        GH_Component _singularComponent;
        Dictionary<string, InputGroup> _input = new Dictionary<string, InputGroup>();
        Dictionary<string, IGH_Param> _output = new Dictionary<string, IGH_Param>();

        public void SetInputs(List<DataTree<ResthopperObject>> values)
        {
            foreach (var tree in values)
            {
                if( !_input.TryGetValue(tree.ParamName, out var inputGroup))
                {
                    continue;
                }

                if (inputGroup.AlreadySet(tree))
                {
                    LogDebug("Skipping input tree... same input");
                    continue;
                }

                inputGroup.CacheTree(tree);

                IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    switch (ParamTypeName(inputGroup.Param))
                    {
                        case "Number":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    double[] doubles = new double[entree.Value.Count];
                                    for (int i = 0; i < doubles.Length; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        doubles[i] = JsonConvert.DeserializeObject<double>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(doubles);
                                    break;
                                }
                            }
                            break;
                        case "Integer":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    int[] integers = new int[entree.Value.Count];
                                    for (int i = 0; i < integers.Length; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        integers[i] = JsonConvert.DeserializeObject<int>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(integers);
                                    break;
                                }
                            }
                            break;
                        case "Point":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    Point3d[] points = new Point3d[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        points[i] = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(points);
                                    break;
                                }
                            }
                            break;
                        case "Line":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    Line[] lines = new Line[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        lines[i] = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                                    }
                                    contextualParameter.AssignContextualData(lines);
                                    break;
                                }
                            }
                            break;
                        case "Text":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    string[] strings = new string[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        strings[i] = restobj.Data.Trim(new char[] { '"' });
                                    }
                                    contextualParameter.AssignContextualData(strings);
                                    break;
                                }
                            }
                            break;
                        case "Geometry":
                            {
                                foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                {
                                    GeometryBase[] geometries = new GeometryBase[entree.Value.Count];
                                    for (int i = 0; i < entree.Value.Count; i++)
                                    {
                                        ResthopperObject restobj = entree.Value[i];
                                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                                        geometries[i] = Rhino.Runtime.CommonObject.FromJSON(dict) as GeometryBase;
                                    }
                                    contextualParameter.AssignContextualData(geometries);
                                    break;
                                }
                            }
                            break;
                    }
                    continue;
                }

                inputGroup.Param.VolatileData.Clear();
                inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet!

                if (inputGroup.Param is Param_Point)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Point3d rPt = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                            GH_Point data = new GH_Point(rPt);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Vector)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Vector3d rhVector = JsonConvert.DeserializeObject<Rhino.Geometry.Vector3d>(restobj.Data);
                            GH_Vector data = new GH_Vector(rhVector);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Integer)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            int rhinoInt = JsonConvert.DeserializeObject<int>(restobj.Data);
                            GH_Integer data = new GH_Integer(rhinoInt);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Number)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                            GH_Number data = new GH_Number(rhNumber);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_String)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            string rhString = restobj.Data;
                            GH_String data = new GH_String(rhString);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Line)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Line rhLine = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                            GH_Line data = new GH_Line(rhLine);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Curve)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
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
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                                var c = (Rhino.Geometry.Curve)Rhino.Runtime.CommonObject.FromJSON(dict);
                                ghCurve = new GH_Curve(c);
                            }
                            inputGroup.Param.AddVolatileData(path, i, ghCurve);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Circle)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Circle rhCircle = JsonConvert.DeserializeObject<Rhino.Geometry.Circle>(restobj.Data);
                            GH_Circle data = new GH_Circle(rhCircle);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Plane)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Plane rhPlane = JsonConvert.DeserializeObject<Rhino.Geometry.Plane>(restobj.Data);
                            GH_Plane data = new GH_Plane(rhPlane);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Rectangle)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Rectangle3d rhRectangle = JsonConvert.DeserializeObject<Rhino.Geometry.Rectangle3d>(restobj.Data);
                            GH_Rectangle data = new GH_Rectangle(rhRectangle);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Box)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Box rhBox = JsonConvert.DeserializeObject<Rhino.Geometry.Box>(restobj.Data);
                            GH_Box data = new GH_Box(rhBox);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Surface)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Surface rhSurface = JsonConvert.DeserializeObject<Rhino.Geometry.Surface>(restobj.Data);
                            GH_Surface data = new GH_Surface(rhSurface);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Brep)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Brep rhBrep = JsonConvert.DeserializeObject<Rhino.Geometry.Brep>(restobj.Data);
                            GH_Brep data = new GH_Brep(rhBrep);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Mesh)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            Rhino.Geometry.Mesh rhMesh = JsonConvert.DeserializeObject<Rhino.Geometry.Mesh>(restobj.Data);
                            GH_Mesh data = new GH_Mesh(rhMesh);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is GH_NumberSlider)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                            GH_Number data = new GH_Number(rhNumber);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is Param_Boolean || inputGroup.Param is GH_BooleanToggle)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            bool boolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                            GH_Boolean data = new GH_Boolean(boolean);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }

                if (inputGroup.Param is GH_Panel)
                {
                    foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                    {
                        GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                        for (int i = 0; i < entree.Value.Count; i++)
                        {
                            ResthopperObject restobj = entree.Value[i];
                            string rhString = JsonConvert.DeserializeObject<string>(restobj.Data);
                            GH_String data = new GH_String(rhString);
                            inputGroup.Param.AddVolatileData(path, i, data);
                        }
                    }
                    continue;
                }
            }

        }

        public Schema Solve()
        {
            HasErrors = false;
            Schema outputSchema = new Schema();
            outputSchema.Algo = "";

            // solve definition
            Definition.Enabled = true;
            Definition.NewSolution(false, GH_SolutionMode.CommandLine);

            LogRuntimeMessages(Definition.ActiveObjects(), outputSchema);

            foreach (var kvp in _output)
            {
                var param = kvp.Value;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new DataTree<ResthopperObject>();
                outputTree.ParamName = kvp.Key;

                var volatileData = param.VolatileData;
                for (int p = 0; p < volatileData.PathCount; p++)
                {
                    var resthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in volatileData.get_Branch(p))
                    {
                        if (goo == null)
                            continue;

                        switch (goo)
                        {
                            case GH_Boolean ghValue:
                                {
                                    bool rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<bool>(rhValue));
                                }
                                break;
                            case GH_Point ghValue:
                                {
                                    Point3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Point3d>(rhValue));
                                }
                                break;
                            case GH_Vector ghValue:
                                {
                                    Vector3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Vector3d>(rhValue));
                                }
                                break;
                            case GH_Integer ghValue:
                                {
                                    int rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<int>(rhValue));
                                }
                                break;
                            case GH_Number ghValue:
                                {
                                    double rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<double>(rhValue));
                                }
                                break;
                            case GH_String ghValue:
                                {
                                    string rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<string>(rhValue));
                                }
                                break;
                            case GH_SubD ghValue:
                                {
                                    SubD rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<SubD>(rhValue));
                                }
                                break;
                            case GH_Line ghValue:
                                {
                                    Line rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Line>(rhValue));
                                }
                                break;
                            case GH_Curve ghValue:
                                {
                                    Curve rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Curve>(rhValue));
                                }
                                break;
                            case GH_Circle ghValue:
                                {
                                    Circle rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Circle>(rhValue));
                                }
                                break;
                            case GH_Plane ghValue:
                                {
                                    Plane rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Plane>(rhValue));
                                }
                                break;
                            case GH_Rectangle ghValue:
                                {
                                    Rectangle3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Rectangle3d>(rhValue));
                                }
                                break;
                            case GH_Box ghValue:
                                {
                                    Box rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Box>(rhValue));
                                }
                                break;
                            case GH_Surface ghValue:
                                {
                                    Brep rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                break;
                            case GH_Brep ghValue:
                                {
                                    Brep rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                break;
                            case GH_Mesh ghValue:
                                {
                                    Mesh rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Mesh>(rhValue));
                                }
                                break;
                        }
                    }

                    GhPath path = new GhPath(new int[] { p });
                    outputTree.Add(path.ToString(), resthopperObjectList);
                }

                outputSchema.Values.Add(outputTree);
            }


            if (outputSchema.Values.Count < 1)
                throw new System.Exceptions.PayAttentionException("Looks like you've missed something..."); // TODO

            return outputSchema;
        }

        private void LogRuntimeMessages(IEnumerable<IGH_ActiveObject> objects, Schema schema)
        {
            foreach (var obj in objects)
            {
                foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Error))
                {
                    string errorMsg = $"{msg}: component \"{obj.NickName}\" ({obj.InstanceGuid})";
                    LogError(errorMsg);
                    schema.Errors.Add(errorMsg);
                    HasErrors = true;
                }
                if (Config.Debug)
                {
                    foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Warning))
                    {
                        string warningMsg = $"{msg}: component \"{obj.NickName}\" ({obj.InstanceGuid})";
                        LogDebug(warningMsg);
                        schema.Warnings.Add(warningMsg);
                    }
                    foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Remark))
                    {
                        LogDebug($"Remark in grasshopper component: \"{obj.NickName}\" ({obj.InstanceGuid}): {msg}");
                    }
                }
            }
        }

        static string ParamTypeName(IGH_Param param)
        {
            Type t = param.GetType();
            // TODO: Figure out why the GetGeometryParameter throws exceptions when calling TypeName
            if (t.Name.Equals("GetGeometryParameter"))
            {
                return "Geometry";
            }
            return param.TypeName;
        }

        public IoResponseSchema GetInputsAndOutputs()
        {
            // Parse input and output names
            List<string> inputNames = new List<string>();
            List<string> outputNames = new List<string>();
            var inputs = new List<InputParamSchema>();
            var outputs = new List<IoParamSchema>();

            foreach (var i in _input)
            {
                inputNames.Add(i.Key);
                var inputSchema = new InputParamSchema
                {
                    Name = i.Key,
                    ParamType = ParamTypeName(i.Value.Param),
                    Description = i.Value.GetDescription(),
                    AtLeast = i.Value.GetAtLeast(),
                    AtMost = i.Value.GetAtMost(),
                    Default = i.Value.GetDefault(),
                    Minimum = i.Value.GetMinimum(),
                    Maximum = i.Value.GetMaximum(),
                };
                if (_singularComponent != null)
                {
                    inputSchema.Description = i.Value.Param.Description;
                    if (i.Value.Param.Access == GH_ParamAccess.item)
                    {
                        inputSchema.AtMost = inputSchema.AtLeast;
                    }
                }
                inputs.Add(inputSchema);
            }

            foreach (var o in _output)
            {
                outputNames.Add(o.Key);
                outputs.Add(new IoParamSchema
                {
                    Name = o.Key,
                    ParamType = o.Value.TypeName
                });
            }

            string description = _singularComponent == null ?
                Definition.Properties.Description :
                _singularComponent.Description;

            return new IoResponseSchema
            {
                Description = description,
                InputNames = inputNames,
                OutputNames = outputNames,
                Inputs = inputs,
                Outputs = outputs
            };
        }

        public static GH_Archive ArchiveFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (File.Exists(url))
            {
                // local file
                var archive = new GH_Archive();
                if (archive.ReadFromFile(url))
                {
                    RegisterFileWatcher(url);
                    return archive;
                }
                return null;
            }

            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                byte[] byteArray = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    byteArray = memStream.ToArray();
                }

                try
                {
                    var byteArchive = new GH_Archive();
                    if (byteArchive.Deserialize_Binary(byteArray))
                        return byteArchive;
                }
                catch (Exception) { }

                var grasshopperXml = StripBom(System.Text.Encoding.UTF8.GetString(byteArray));
                var xmlArchive = new GH_Archive();
                if (xmlArchive.Deserialize_Xml(grasshopperXml))
                    return xmlArchive;
            }
            return null;
        }

        public static GH_Archive ArchiveFromBase64String(string blob)
        {
            if (string.IsNullOrWhiteSpace(blob))
                return null;

            byte[] byteArray = Convert.FromBase64String(blob);
            try
            {
                var byteArchive = new GH_Archive();
                if (byteArchive.Deserialize_Binary(byteArray))
                    return byteArchive;
            }
            catch (Exception) { }

            var grasshopperXml = StripBom(System.Text.Encoding.UTF8.GetString(byteArray));
            var xmlArchive = new GH_Archive();
            if (xmlArchive.Deserialize_Xml(grasshopperXml))
                return xmlArchive;

            return null;
        }

        // strip bom from string -- [239, 187, 191] in byte array == (char)65279
        // https://stackoverflow.com/a/54894929/1902446
        static string StripBom(string str)
        {
            if (!string.IsNullOrEmpty(str) && str[0] == (char)65279)
                str = str.Substring(1);
            return str;
        }

        static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;
            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v, GeometryResolver.Settings);
            return rhObj;
        }

        class InputGroup
        {
            object _default = null;
            public InputGroup(IGH_Param param)
            {
                Param = param;
                _default = GetDefaultValueHelper(param, 0);
            }

            object GetDefaultValueHelper(IGH_Param param, int depth)
            {
                switch (param)
                {
                    case Grasshopper.Kernel.IGH_ContextualParameter _:
                        {
                            if (0 == depth && param.Sources.Count == 1)
                            {
                                var sourceParam = param.Sources[0];
                                return GetDefaultValueHelper(sourceParam, depth + 1);
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Arc _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Boolean paramBool:
                        if (paramBool.PersistentDataCount == 1)
                            return paramBool.PersistentData[0][0].Value;
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Box _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Brep _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Circle _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Colour _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Complex _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Culture _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Curve _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Field _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_FilePath _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Geometry _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Group _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Guid _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Integer paramInt:
                        if (paramInt.PersistentDataCount == 1)
                            return paramInt.PersistentData[0][0].Value;
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Line _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Matrix _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Mesh _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Number paramNumber:
                        if (paramNumber.PersistentDataCount == 1)
                            return paramNumber.PersistentData[0][0].Value;
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                    case Grasshopper.Kernel.Parameters.Param_Plane paramPlane:
                        if (paramPlane.PersistentDataCount == 1)
                            return paramPlane.PersistentData[0][0].Value;
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Point paramPoint:
                        if (paramPoint.PersistentDataCount == 1)
                            return paramPoint.PersistentData[0][0].Value;
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                    case Grasshopper.Kernel.Parameters.Param_String _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_SubD _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Surface _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Time _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Transform _:
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Vector paramVector:
                        if (paramVector.PersistentDataCount == 1)
                            return paramVector.PersistentData[0][0].Value;
                        break;
                    case Grasshopper.Kernel.Special.GH_NumberSlider paramSlider:
                        return paramSlider.CurrentValue;
                }
                return null;
            }

            public IGH_Param Param { get; }

            public string GetDescription()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    return contextualParameter.Prompt;
                }
                return null;
            }

            public int GetAtLeast()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if(contextualParameter!=null)
                {
                    return contextualParameter.AtLeast;
                }
                return 1;
            }

            public int GetAtMost()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    return contextualParameter.AtMost;
                }
                if (Param is GH_NumberSlider)
                    return 1;
                return int.MaxValue;
            }
            
            public object GetDefault()
            {
                return _default;
            }

            public object GetMinimum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter && p.Sources.Count == 1)
                {
                    p = p.Sources[0];
                }

                if (p is GH_NumberSlider paramSlider)
                    return paramSlider.Slider.Minimum;
                return null;
            }

            public object GetMaximum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter && p.Sources.Count == 1)
                {
                    p = p.Sources[0];
                }

                if (p is GH_NumberSlider paramSlider)
                    return paramSlider.Slider.Maximum;

                return null;
            }

            public bool AlreadySet(DataTree<ResthopperObject> tree)
            {
                if (_tree == null)
                    return false;

                var oldDictionary = _tree.InnerTree;
                var newDictionary = tree.InnerTree;

                if (!oldDictionary.Keys.SequenceEqual(newDictionary.Keys))
                {
                    return false;
                }

                foreach (var kvp in oldDictionary)
                {
                    var oldValue = kvp.Value;
                    if (!newDictionary.TryGetValue(kvp.Key, out List<ResthopperObject> newValue))
                        return false;

                    if (!newValue.SequenceEqual(oldValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            public void CacheTree(DataTree<ResthopperObject> tree)
            {
                _tree = tree;
            }

            DataTree<ResthopperObject> _tree;
        }
    }
}
