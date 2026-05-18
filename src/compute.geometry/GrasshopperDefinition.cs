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
using Serilog;
using System.Reflection;

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

        public static void LogDebug(string message) { Log.Debug(message); }
        public static void LogError(string message) { Log.Error(message); }

        public static GrasshopperDefinition FromUrl(string url, bool cache)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
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
                DataCache.SetCachedDefinition(url, rc, null);
                rc.InDataCache = true;
            }
            return rc;
        }

        public static GrasshopperDefinition FromBase64String(string data, bool cache)
        {
            var archive = ArchiveFromBase64String(data);
            if (archive == null)
                return null;

            var rc = Construct(archive);
            if (rc!=null)
            {
                rc.CacheKey = DataCache.CreateCacheKey(data);
                if (cache)
                {
                    DataCache.SetCachedDefinition(rc.CacheKey, rc, data);
                    rc.InDataCache = true;
                }
            }
            return rc;
        }

        private static GrasshopperDefinition Construct(Guid componentId)
        {
            var component = Grasshopper.Instances.ComponentServer.EmitObject(componentId) as GH_Component;
            if (component==null)
                return null;

            var definition = new GH_Document();
            definition.AddObject(component, false);

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(definition);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception in DocumentAdded event handler");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(definition, null);
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
        private static void AddInput(IGH_Param param, string name, ref GrasshopperDefinition rc)
        {
            if (rc._input.ContainsKey(name))
            {
                string msg = "Multiple input parameters with the same name were detected. Parameter names must be unique.";
                rc.HasErrors = true;
                rc.ErrorMessages.Add(msg);
                LogError(msg);
            }   
            else
                rc._input[name] = new InputGroup(param);
        }
        private static void AddOutput(IGH_Param param, string name, ref GrasshopperDefinition rc)
        {
            if (rc._output.ContainsKey(name))
            {
                string msg = "Multiple output parameters with the same name were detected. Parameter names must be unique.";
                rc.HasErrors = true;
                rc.ErrorMessages.Add(msg);
                LogError(msg);
            }  
            else
                rc._output[name] = param;
        }

        private static GrasshopperDefinition Construct(GH_Archive archive)
        {
            string icon = null;
            var chunk = archive.GetRootNode.FindChunk("Definition");
            if (chunk!=null)
            {
                chunk = chunk.FindChunk("DefinitionProperties");
                if (chunk != null)
                {
                    string s = String.Empty;
                    if (chunk.TryGetString("IconImageData", ref s))
                    {
                        icon = s;
                    }
                }
            }

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception("Unable to extract definition from archive");

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(definition);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception in DocumentAdded event handler");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(definition, icon);
            foreach( var obj in definition.Objects)
            {
                IGH_ContextualParameter contextualParam = obj as IGH_ContextualParameter;
                if (contextualParam != null)
                {
                    IGH_Param param = obj as IGH_Param;          
                    if (param != null && !param.Locked)
                    {
                        AddInput(param, param.NickName, ref rc);
                    }
                    continue;
                }

                Type objectClass = obj.GetType();
                var className = objectClass.Name;
                if (className == "ContextBakeComponent")
                {
                    var contextBaker = obj as GH_Component;
                    if (contextBaker != null && !contextBaker.Locked)
                    {
                        IGH_Param param = contextBaker.Params.Input[0];
                        AddOutput(param, param.NickName, ref rc);
                    }
                }

                if (className == "ContextPrintComponent")
                {
                    var contextPrinter = obj as GH_Component;
                    if (contextPrinter != null && !contextPrinter.Locked)
                    {
                        IGH_Param param = contextPrinter.Params.Input[0];
                        AddOutput(param, param.NickName, ref rc);
                    }  
                }

                var group = obj as GH_Group;
                if (group == null)
                    continue;

                string nickname = group.NickName;
                var groupObjects = group.Objects();
                if ( nickname.Contains("RH_IN") && groupObjects.Count>0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null && !param.Locked)
                    {
                        AddInput(param, nickname, ref rc);
                    }
                }

                if (nickname.Contains("RH_OUT") && groupObjects.Count > 0)
                {
                    if (groupObjects[0] is IGH_Param param)
                    {
                        AddOutput(param, nickname, ref rc);
                    }
                    else if(groupObjects[0] is GH_Component component)
                    {
                        int outputCount = component.Params.Output.Count;
                        for(int i=0; i<outputCount; i++)
                        {
                            if(1==outputCount)
                            {
                                AddOutput(component.Params.Output[i], nickname, ref rc);
                            }
                            else
                            {
                                string itemName = $"{nickname} ({component.Params.Output[i].NickName})";
                                AddOutput(component.Params.Output[i], itemName, ref rc);
                            }
                        }
                    }
                }
            }
            return rc;
        }

        private GrasshopperDefinition(GH_Document definition, string icon)
        {
            Definition = definition;
            _iconString = icon;
            FileRuntimeCacheSerialNumber = _watchedFileRuntimeSerialNumber;
        }

        public GH_Document Definition { get; }
        public bool InDataCache { get; set; }
        public bool HasErrors { get; private set; } // default: false
        public bool IsLocalFileDefinition { get; set; } // default: false
        public uint FileRuntimeCacheSerialNumber { get; private set; }
        public string CacheKey { get; set; }
        string _iconString;
        GH_Component _singularComponent;
        Dictionary<string, InputGroup> _input = new Dictionary<string, InputGroup>();
        Dictionary<string, IGH_Param> _output = new Dictionary<string, IGH_Param>();
        public List<string> ErrorMessages = new List<string>();

        public GH_Path GetPath(string p)
        {
            string tempPath = p.Trim('{', '}');
            int[] pathIndices = tempPath.Split(';').Select(Int32.Parse).ToArray();
            return new GH_Path(pathIndices);
        }

        public void SetInputs(List<Resthopper.IO.DataTree<ResthopperObject>> values)
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
                    if (contextualParameter.AtLeast == 0)
                        (contextualParameter as IGH_Param).Optional = true;

                    switch (ParamTypeName(inputGroup.Param))
                    {
                        case "Boolean":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Boolean(JsonConvert.DeserializeObject<bool>(r.Data)));
                            break;
                        case "Number":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Number(JsonConvert.DeserializeObject<double>(r.Data)));
                            break;
                        case "Integer":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Integer(JsonConvert.DeserializeObject<int>(r.Data)));
                            break;
                        case "Point":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Point(JsonConvert.DeserializeObject<Point3d>(r.Data)));
                            break;
                        case "Plane":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Plane(JsonConvert.DeserializeObject<Plane>(r.Data)));
                            break;
                        case "Line":
                            BuildAndAssignContextualTree(contextualParameter, tree, r => new GH_Line(JsonConvert.DeserializeObject<Line>(r.Data)));
                            break;
                        case "Text":
                            BuildAndAssignContextualTree(contextualParameter, tree, DeserializeText);
                            break;
                        case "Geometry":
                            BuildAndAssignContextualTree(contextualParameter, tree, DeserializeGeometry);
                            break;
                    }
                    continue;
                }
                
                inputGroup.Param.VolatileData.Clear();
                inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet!

                Func<ResthopperObject, IGH_Goo> convert = inputGroup.Param switch
                {
                    Param_Point _      => r => new GH_Point(JsonConvert.DeserializeObject<Point3d>(r.Data)),
                    Param_Vector _     => r => new GH_Vector(JsonConvert.DeserializeObject<Vector3d>(r.Data)),
                    Param_Integer _    => r => new GH_Integer(JsonConvert.DeserializeObject<int>(r.Data)),
                    Param_Number _     => r => new GH_Number(JsonConvert.DeserializeObject<double>(r.Data)),
                    Param_String _     => r => new GH_String(r.Data),
                    Param_Line _       => r => new GH_Line(JsonConvert.DeserializeObject<Line>(r.Data)),
                    Param_Curve _      => DeserializeCurve,
                    Param_Circle _     => r => new GH_Circle(JsonConvert.DeserializeObject<Circle>(r.Data)),
                    Param_Plane _      => r => new GH_Plane(JsonConvert.DeserializeObject<Plane>(r.Data)),
                    Param_Rectangle _  => r => new GH_Rectangle(JsonConvert.DeserializeObject<Rectangle3d>(r.Data)),
                    Param_Box _        => r => new GH_Box(JsonConvert.DeserializeObject<Box>(r.Data)),
                    Param_Surface _    => r => new GH_Surface(JsonConvert.DeserializeObject<Surface>(r.Data)),
                    Param_Brep _       => r => new GH_Brep(JsonConvert.DeserializeObject<Brep>(r.Data)),
                    Param_Mesh _       => r => new GH_Mesh(JsonConvert.DeserializeObject<Mesh>(r.Data)),
                    GH_NumberSlider _  => r => new GH_Number(JsonConvert.DeserializeObject<double>(r.Data)),
                    Param_Boolean _ or GH_BooleanToggle _ => r => new GH_Boolean(JsonConvert.DeserializeObject<bool>(r.Data)),
                    GH_Panel _         => r => new GH_String(JsonConvert.DeserializeObject<string>(r.Data)),
                    _                  => null
                };
                if (convert != null)
                    AddTreeData(inputGroup.Param, tree, convert);
            }

        }

        // Shared write path for SetInputs' regular-parameter dispatch. Each Param_X case
        // differs only in how a ResthopperObject is deserialized + wrapped, so the converter
        // delegate captures the per-type logic and this helper handles the path/index walk.
        static void AddTreeData(IGH_Param param, Resthopper.IO.DataTree<ResthopperObject> tree, Func<ResthopperObject, IGH_Goo> convert)
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                for (int i = 0; i < entree.Value.Count; i++)
                    param.AddVolatileData(path, i, convert(entree.Value[i]));
            }
        }

        // Curves arrive in one of two JSON shapes: a Polyline (which we wrap as a PolylineCurve)
        // or a Rhino CommonObject dictionary that FromJSON can re-hydrate into any Curve subtype.
        static IGH_Goo DeserializeCurve(ResthopperObject restobj)
        {
            try
            {
                Polyline data = JsonConvert.DeserializeObject<Polyline>(restobj.Data);
                return new GH_Curve(new PolylineCurve(data));
            }
            catch
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                var c = (Curve)Rhino.Runtime.CommonObject.FromJSON(dict);
                return new GH_Curve(c);
            }
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo> _assignContextualDataTreeMethods = new System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo>();

        void BuildAndAssignContextualTree<T>(IGH_ContextualParameter param,
                                             Resthopper.IO.DataTree<ResthopperObject> tree,
                                             Func<ResthopperObject, T> convert)
        {
            var inputTree = new Grasshopper.DataTree<T>();
            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = GetPath(entree.Key);
                foreach (var restobj in entree.Value)
                    inputTree.Add(convert(restobj), path);
            }
            var method = _assignContextualDataTreeMethods.GetOrAdd(
                param.GetType(),
                t => t.GetMethod("AssignContextualDataTree"));
            method?.Invoke(param, new object[] { inputTree });
        }

        static GH_String DeserializeText(ResthopperObject restobj)
        {
            try
            {
                return new GH_String(JsonConvert.DeserializeObject<string>(restobj.Data));
            }
            catch (Exception)
            {
                return new GH_String(System.Text.RegularExpressions.Regex.Unescape(restobj.Data));
            }
        }

        // Geometry contextual inputs arrive as a Rhino CommonObject JSON dictionary; rehydrate
        // to GeometryBase then wrap as IGH_GeometricGoo. Original code did not null-check before
        // adding to the tree, so we don't either (preserving behavior).
        static IGH_GeometricGoo DeserializeGeometry(ResthopperObject restobj)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
            var gb = Rhino.Runtime.CommonObject.FromJSON(dict) as GeometryBase;
            return GH_Convert.ToGeometricGoo(gb);
        }

        public Schema Solve(int rhinoVersion)
        {
            HasErrors = false;
            Schema outputSchema = new Schema();
            outputSchema.Algo = "";

            // solve definition
            Definition.Enabled = true;
            Definition.NewSolution(false, GH_SolutionMode.CommandLine);

            foreach(string msg in ErrorMessages)
            {
                outputSchema.Errors.Add(msg);
            }

            LogRuntimeMessages(Definition.ActiveObjects(), outputSchema);

            foreach (var kvp in _output)
            {
                var param = kvp.Value;
                if (param == null)
                    continue;

                Resthopper.IO.DataTree<ResthopperObject> outputTree = SerializeDataTree(param.VolatileData, kvp.Key, rhinoVersion) as Resthopper.IO.DataTree<ResthopperObject>;
                outputSchema.Values.Add(outputTree);
            }

            if (outputSchema.Values.Count < 1)
                throw new System.Exceptions.PayAttentionException("Looks like you've missed something..."); // TODO

            // Setting warnings and errors to null ever so slightly shrinks down the json sent back to the client
            if (outputSchema.Warnings.Count < 1)
                outputSchema.Warnings = null;
            if (outputSchema.Errors.Count < 1)
                outputSchema.Errors = null;

            return outputSchema;
        }

        private static object SerializeDataTree(IGH_Structure data, string name, int rhinoVersion = 7)
        {
            // Get data
            var outputTree = new Resthopper.IO.DataTree<ResthopperObject>();
            outputTree.ParamName = name;

            foreach (var path in data.Paths)
            {
                var resthopperObjectList = new List<ResthopperObject>();
                foreach (var goo in data.get_Branch(path))
                {
                    if (goo == null)
                        continue;

                    // GH_Surface unwraps to Brep (its .Value is a Brep), preserved from the original.
                    // Unrecognized goo types are silently skipped, matching the original switch's
                    // implicit fall-through to no-op.
                    ResthopperObject resthopperObject = goo switch
                    {
                        GH_Boolean g           => GetResthopperObject<bool>(g.Value, rhinoVersion),
                        GH_Point g             => GetResthopperObject<Point3d>(g.Value, rhinoVersion),
                        GH_Vector g            => GetResthopperObject<Vector3d>(g.Value, rhinoVersion),
                        GH_Integer g           => GetResthopperObject<int>(g.Value, rhinoVersion),
                        GH_Number g            => GetResthopperObject<double>(g.Value, rhinoVersion),
                        GH_String g            => GetResthopperObject<string>(g.Value, rhinoVersion),
                        GH_SubD g              => GetResthopperObject<SubD>(g.Value, rhinoVersion),
                        GH_Line g              => GetResthopperObject<Line>(g.Value, rhinoVersion),
                        GH_Curve g             => GetResthopperObject<Curve>(g.Value, rhinoVersion),
                        GH_Circle g            => GetResthopperObject<Circle>(g.Value, rhinoVersion),
                        GH_Arc g               => GetResthopperObject<Arc>(g.Value, rhinoVersion),
                        GH_Plane g             => GetResthopperObject<Plane>(g.Value, rhinoVersion),
                        GH_Rectangle g         => GetResthopperObject<Rectangle3d>(g.Value, rhinoVersion),
                        GH_Box g               => GetResthopperObject<Box>(g.Value, rhinoVersion),
                        GH_Surface g           => GetResthopperObject<Brep>(g.Value, rhinoVersion),
                        GH_Brep g              => GetResthopperObject<Brep>(g.Value, rhinoVersion),
                        GH_Mesh g              => GetResthopperObject<Mesh>(g.Value, rhinoVersion),
                        GH_Extrusion g         => GetResthopperObject<Extrusion>(g.Value, rhinoVersion),
                        GH_PointCloud g        => GetResthopperObject<PointCloud>(g.Value, rhinoVersion),
                        GH_InstanceReference g => GetResthopperObject<InstanceReferenceGeometry>(g.Value, rhinoVersion),
                        GH_Hatch g             => GetResthopperObject<Hatch>(g.Value, rhinoVersion),
                        GH_LinearDimension g   => GetResthopperObject<LinearDimension>(g.Value, rhinoVersion),
                        GH_RadialDimension g   => GetResthopperObject<RadialDimension>(g.Value, rhinoVersion),
                        GH_AngularDimension g  => GetResthopperObject<AngularDimension>(g.Value, rhinoVersion),
                        GH_OrdinateDimension g => GetResthopperObject<OrdinateDimension>(g.Value, rhinoVersion),
                        GH_Leader g            => GetResthopperObject<Leader>(g.Value, rhinoVersion),
                        GH_TextEntity g        => GetResthopperObject<TextEntity>(g.Value, rhinoVersion),
                        GH_TextDot g           => GetResthopperObject<TextDot>(g.Value, rhinoVersion),
                        GH_Centermark g        => GetResthopperObject<Centermark>(g.Value, rhinoVersion),
                        _                      => null
                    };
                    if (resthopperObject != null)
                        resthopperObjectList.Add(resthopperObject);
                }
                // preserve paths when returning data
                outputTree.Add(path.ToString(), resthopperObjectList);
            }
            return outputTree;
        }

        private void LogRuntimeMessages(IEnumerable<IGH_ActiveObject> objects, Schema schema)
        {
            foreach (var obj in objects)
            {
                foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Error))
                {
                    string errorMsg = $"{msg}: component \"{obj.Name}\" ({obj.InstanceGuid})";
                    LogError(errorMsg);
                    schema.Errors.Add(errorMsg);
                    HasErrors = true;
                }
                if (Config.Debug)
                {
                    foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Warning))
                    {
                        string warningMsg = $"{msg}: component \"{obj.Name}\" ({obj.InstanceGuid})";
                        Log.Warning(warningMsg);
                        schema.Warnings.Add(warningMsg);
                    }
                    foreach (var msg in obj.RuntimeMessages(GH_RuntimeMessageLevel.Remark))
                    {
                        LogDebug($"Remark in grasshopper component: \"{obj.Name}\" ({obj.InstanceGuid}): {msg}");
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

        public string GetIconAsString()
        {
            if (!string.IsNullOrWhiteSpace(_iconString))
                return _iconString;

            System.Drawing.Bitmap bmp = null;
            if (_singularComponent!=null)
            {
                bmp = _singularComponent.Icon_24x24;
            }

            if (bmp!=null)
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] bytes = ms.ToArray();
                    string rc = Convert.ToBase64String(bytes);
                    _iconString = rc;
                    return rc;
                }
            }
            return null;
        }

        public IoResponseSchema GetInputsAndOutputs()
        {
            // Parse input and output names
            List<string> inputNames = new List<string>();
            List<string> outputNames = new List<string>();
            var inputs = new List<InputParamSchema>();
            var outputs = new List<IoParamSchema>();

            var sortedInputs = from x in _input orderby x.Value.Param.Attributes.Pivot.Y select x;
            var sortedOutputs = from x in _output orderby x.Value.Attributes.Pivot.Y select x;

            foreach (var i in sortedInputs)
            {
                inputNames.Add(i.Key);
                var inputSchema = new InputParamSchema
                {
                    Name = i.Key,
                    ParamType = ParamTypeName(i.Value.Param),
                    Description = i.Value.GetDescription(),
                    AtLeast = i.Value.GetAtLeast(),
                    AtMost = i.Value.GetAtMost(),
                    TreeAccess = i.Value.GetTreeAccess(),
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

            foreach (var o in sortedOutputs)
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

        static ResthopperObject GetResthopperObject<T>(object goo, int rhinoVerion)
        {
            var v = (T)goo;
            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;

            if (v is GeometryBase geometry)
                rhObj.Data = geometry.ToJSON(new Rhino.FileIO.SerializationOptions() { RhinoVersion = rhinoVerion });
            else
                rhObj.Data = JsonConvert.SerializeObject(v, GeometryResolver.Settings(rhinoVerion));

            return rhObj;
        }

        class InputGroup
        {
            object _default = null;
            public InputGroup(IGH_Param param)
            {
                Param = param;

                param.ClearData();
                param.CollectData();
                _default = SerializeDataTree(param.VolatileData, param.Name);
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

            public bool GetTreeAccess()
            {
                IGH_ContextualParameter contextualParameter = Param as IGH_ContextualParameter;
                if (contextualParameter != null)
                {
                    var result = contextualParameter.GetType().GetProperty("TreeAccess")?.GetValue(contextualParameter, null);
                    if(result != null)
                        return (bool)result;
                }
                return false;
            }

            public object GetDefault()
            {
                return _default;
            }

            public double? GetMinimum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter)
                {
                    var par = p as IGH_ContextualParameter;
                    var pTypeName = ParamTypeName(p);
                    var pType = par.GetType();
                    var props = pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
                    var info = props.FirstOrDefault(x => x.Name == "Minimum");
                    if(info != null)
                    {
                        var val = info.GetValue(par, null);
                        if (val != null)
                        {
                            var min = Convert.ToDouble(val);
                            if (pTypeName == "Integer")
                            {
                                if (min > int.MinValue + Rhino.RhinoMath.Epsilon)
                                    return min;
                            }
                            else if (pTypeName == "Number")
                            {
                                if (min > double.MinValue + Rhino.RhinoMath.Epsilon)
                                    return min;
                            }
                        }
                    }

                    if (p.Sources.Count == 1)
                        p = p.Sources[0];
                }
                
                if (p is GH_NumberSlider paramSlider)
                    return (double)paramSlider.Slider.Minimum;
                return null;
            }

            public double? GetMaximum()
            {
                var p = Param;
                if (p is IGH_ContextualParameter)
                {
                    var par = p as IGH_ContextualParameter;
                    var pType = par.GetType();
                    var pTypeName = ParamTypeName(p);
                    var props = pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
                    var info = props.FirstOrDefault(x => x.Name == "Maximum");
                    if(info != null)
                    {
                        var val = info.GetValue(par, null);
                        if (val != null)
                        {
                            var max = Convert.ToDouble(val);
                            if (pTypeName == "Integer")
                            {
                                if (max < int.MaxValue - Rhino.RhinoMath.Epsilon)
                                    return max;
                            }
                            else if (pTypeName == "Number")
                            {
                                if (max < double.MaxValue - Rhino.RhinoMath.Epsilon)
                                    return max;
                            }
                        }
                    }

                    if (p.Sources.Count == 1)
                        p = p.Sources[0];
                }

                if (p is GH_NumberSlider paramSlider)
                    return (double)paramSlider.Slider.Maximum;

                return null;
            }

            public bool AlreadySet(Resthopper.IO.DataTree<ResthopperObject> tree)
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

            public void CacheTree(Resthopper.IO.DataTree<ResthopperObject> tree)
            {
                _tree = tree;
            }

            Resthopper.IO.DataTree<ResthopperObject> _tree;
        }
    }
}
