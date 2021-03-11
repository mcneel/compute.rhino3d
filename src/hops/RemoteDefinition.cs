using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using Resthopper.IO;
using System.IO;

namespace Compute.Components
{
    class RemoteDefinition : IDisposable
    {
        enum PathType
        {
            GrasshopperDefinition,
            ComponentGuid,
            Server,
            NonresponsiveUrl
        }

        HopsComponent _parentComponent;
        Dictionary<string, Tuple<InputParamSchema, IGH_Param>> _inputParams;
        Dictionary<string, IGH_Param> _outputParams;
        string _description = null;
        System.Drawing.Bitmap _customIcon = null;
        string _path = null;
        string _cacheKey = null;
        PathType? _pathType;

        public static RemoteDefinition Create(string path, HopsComponent parentComponent)
        {
            var rc = new RemoteDefinition(path, parentComponent);
            RemoteDefinitionCache.Add(rc);
            return rc;
        }

        private RemoteDefinition(string path, HopsComponent parentComponent)
        {
            _parentComponent = parentComponent;
            _path = path;
        }

        public void Dispose()
        {
            _parentComponent = null;
            RemoteDefinitionCache.Remove(this);
        }

        PathType GetPathType()
        {
            if (!_pathType.HasValue)
            {
                if (Guid.TryParse(_path, out Guid id))
                {
                    _pathType = PathType.ComponentGuid;
                }
                else
                {
                    _pathType = PathType.GrasshopperDefinition;
                    if (_path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var getTask = HttpClient.GetAsync(_path);
                            var response = getTask.Result;
                            string mediaType = response.Content.Headers.ContentType.MediaType.ToLowerInvariant();
                            if (mediaType.Contains("json"))
                                _pathType = PathType.Server;
                        }
                        catch (Exception)
                        {
                            _pathType = PathType.NonresponsiveUrl;
                        }
                    }
                }
            }
            return _pathType.Value;
        }

        public string Path { get { return _path; } }

        public void OnWatchedFileChanged()
        {
            _cacheKey = null;
            _description = null;
            if (_parentComponent != null)
                _parentComponent.OnRemoteDefinitionChanged();
        }

        public Dictionary<string, Tuple<InputParamSchema, IGH_Param>> GetInputParams()
        {
            if( _inputParams == null)
            {
                GetRemoteDescription();
            }
            return _inputParams;
        }

        public Dictionary<string, IGH_Param> GetOutputParams()
        {
            if (_outputParams == null)
            {
                GetRemoteDescription();
            }
            return _outputParams;
        }

        public string GetDescription(out System.Drawing.Bitmap customIcon)
        {
            if (_description == null)
            {
                GetRemoteDescription();
            }
            customIcon = _customIcon;
            return _description;
        }

        void GetRemoteDescription()
        {
            bool performPost = false;

            string address = null;
            var pathType = GetPathType();
            switch (pathType)
            {
                case PathType.GrasshopperDefinition:
                    {
                        if (Path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                            File.Exists(Path))
                        {
                            address = Path;
                            performPost = true;
                        }
                    }
                    break;
                case PathType.ComponentGuid:
                    address = Servers.GetDescriptionUrl(Guid.Parse(Path));
                    break;
                case PathType.Server:
                    address = Path;
                    break;
                case PathType.NonresponsiveUrl:
                    break;
            }
            if (address == null)
                return;

            IoResponseSchema responseSchema = null;
            System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask = null;
            IDisposable contentToDispose = null;
            if (performPost)
            {
                string postUrl = Servers.GetDescriptionPostUrl();
                var bytes = System.IO.File.ReadAllBytes(address);
                var schema = new Schema();
                schema.Algo = Convert.ToBase64String(bytes);
                string inputJson = JsonConvert.SerializeObject(schema);
                var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                responseTask = HttpClient.PostAsync(postUrl, content);
                contentToDispose = content;
            }
            else
            {
                responseTask = HttpClient.GetAsync(address);
            }

            if (responseTask != null)
            {
                var responseMessage = responseTask.Result;
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(stringResult))
                {
                    responseSchema = JsonConvert.DeserializeObject<Resthopper.IO.IoResponseSchema>(stringResult);
                    _cacheKey = responseSchema.CacheKey;
                }
            }

            if (contentToDispose != null)
                contentToDispose.Dispose();

            if (responseSchema != null)
            { 
                _description = responseSchema.Description;
                _customIcon = null;
                if (!string.IsNullOrWhiteSpace(responseSchema.Icon))
                {
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(responseSchema.Icon);
                        using (var ms = new MemoryStream(bytes))
                        {
                            _customIcon = new System.Drawing.Bitmap(ms);
                        }
                    }
                    catch(Exception)
                    {
                    }
                }
                _inputParams = new Dictionary<string, Tuple<InputParamSchema, IGH_Param>>();
                _outputParams = new Dictionary<string, IGH_Param>();
                foreach (var input in responseSchema.Inputs)
                {
                    string inputParamName = input.Name;
                    if (inputParamName.StartsWith("RH_IN:"))
                    {
                        var chunks = inputParamName.Split(new char[] { ':' });
                        inputParamName = chunks[chunks.Length - 1];
                    }
                    _inputParams[inputParamName] = Tuple.Create(input, ParamFromIoResponseSchema(input));
                }
                foreach (var output in responseSchema.Outputs)
                {
                    string outputParamName = output.Name;
                    if (outputParamName.StartsWith("RH_OUT:"))
                    {
                        var chunks = outputParamName.Split(new char[] { ':' });
                        outputParamName = chunks[chunks.Length - 1];
                    }
                    _outputParams[outputParamName] = ParamFromIoResponseSchema(output);
                }
            }
        }

        static System.Net.Http.HttpClient _httpClient = null;
        static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (_httpClient==null)
                {
                    _httpClient = new System.Net.Http.HttpClient();
                }
                return _httpClient;
            }
        }

        public Schema Solve(Schema inputSchema)
        {
            string solveUrl;
            var pathType = GetPathType();
            if (pathType == PathType.NonresponsiveUrl)
                return null;

            if (pathType == PathType.GrasshopperDefinition || pathType == PathType.ComponentGuid)
            {
                solveUrl = Servers.GetSolveUrl();
                if (!string.IsNullOrEmpty(_cacheKey))
                    inputSchema.Pointer = _cacheKey;
            }
            else
            {
                int index = Path.LastIndexOf('/');
                solveUrl = Path.Substring(0, index + 1) + "solve";
            }

            string inputJson = JsonConvert.SerializeObject(inputSchema);

            using (var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json"))
            {
                var postTask = HttpClient.PostAsync(solveUrl, content);
                var responseMessage = postTask.Result;
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    if (File.Exists(Path) && string.IsNullOrEmpty(inputSchema.Algo))
                    {
                        var bytes = System.IO.File.ReadAllBytes(Path);
                        string base64 = Convert.ToBase64String(bytes);
                        inputSchema.Algo = base64;
                        inputJson = JsonConvert.SerializeObject(inputSchema);
                        var content2 = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                        postTask = HttpClient.PostAsync(solveUrl, content2);
                        responseMessage = postTask.Result;
                        if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                            throw new Exception("Unable to solve on compute");
                    }
                }
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                var schema = JsonConvert.DeserializeObject<Resthopper.IO.Schema>(stringResult);
                _cacheKey = schema.Pointer;
                return schema;
            }
        }

        public void SetComponentOutputs(Schema schema, IGH_DataAccess DA, List<IGH_Param> outputParams, GH_ActiveObject component)
        {
            foreach (var datatree in schema.Values)
            {
                string outputParamName = datatree.ParamName;
                if (outputParamName.StartsWith("RH_OUT:"))
                {
                    var chunks = outputParamName.Split(new char[] { ':' });
                    outputParamName = chunks[chunks.Length - 1];
                }
                int paramIndex = 0;
                for (int i = 0; i < outputParams.Count; i++)
                {
                    if (outputParams[i].Name.Equals(outputParamName))
                    {
                        paramIndex = i;
                        break;
                    }
                }

                var structure = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
                Grasshopper.Kernel.Types.IGH_Goo singleGoo = null;
                foreach (var kv in datatree.InnerTree)
                {
                    var tokens = kv.Key.Trim(new char[] { '{', '}' }).Split(';');
                    List<int> elements = new List<int>();
                    foreach (var token in tokens)
                    {
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            elements.Add(int.Parse(token));
                        }
                    }

                    var path = new Grasshopper.Kernel.Data.GH_Path(elements.ToArray());
                    for (int gooIndex = 0; gooIndex < kv.Value.Count; gooIndex++)
                    {
                        var goo = GooFromReshopperObject(kv.Value[gooIndex]);
                        singleGoo = goo;
                        structure.Insert(goo, path, gooIndex);
                    }
                }
                if (structure.DataCount == 1)
                    DA.SetData(paramIndex, singleGoo);
                else if (structure.PathCount == 1)
                    DA.SetDataList(paramIndex, structure.AllData(false)); // let grasshopper handle paths
                else
                    DA.SetDataTree(paramIndex, structure);
            }

            foreach (var error in schema.Errors)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            }
            foreach (var warning in schema.Warnings)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }
        }

        static Grasshopper.Kernel.Types.IGH_Goo GooFromReshopperObject(ResthopperObject obj)
        {
            string data = obj.Data.Trim('"');

            if (obj.Type.StartsWith("Rhino.Geometry"))
            {
                var pt = new Rhino.Geometry.Point3d();
                string s = pt.GetType().AssemblyQualifiedName;
                int index = s.IndexOf(",");
                string sType = $"{obj.Type}{s.Substring(index)}";

                System.Type type = System.Type.GetType(sType);
                if (type != null && typeof(GeometryBase).IsAssignableFrom(type))
                {
                    Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                    var geometry = Rhino.Runtime.CommonObject.FromJSON(dict);
                    Surface surface = geometry as Surface;
                    if (surface != null)
                        geometry = surface.ToBrep();
                    if (geometry is Brep)
                        return new Grasshopper.Kernel.Types.GH_Brep(geometry as Brep);
                    if (geometry is Curve)
                        return new Grasshopper.Kernel.Types.GH_Curve(geometry as Curve);
                    if (geometry is Mesh)
                        return new Grasshopper.Kernel.Types.GH_Mesh(geometry as Mesh);
                    if (geometry is SubD)
                        return new Grasshopper.Kernel.Types.GH_SubD(geometry as SubD);
                }
            }

            switch (obj.Type)
            {
                case "System.Double":
                    return new Grasshopper.Kernel.Types.GH_Number(double.Parse(data));
                case "System.String":
                    return new Grasshopper.Kernel.Types.GH_String(data);
                case "System.Int32":
                    return new Grasshopper.Kernel.Types.GH_Integer(int.Parse(data));
                case "Rhino.Geometry.Circle":
                    return new Grasshopper.Kernel.Types.GH_Circle(JsonConvert.DeserializeObject<Circle>(data));
                case "Rhino.Geometry.Line":
                    return new Grasshopper.Kernel.Types.GH_Line(JsonConvert.DeserializeObject<Line>(data));
                case "Rhino.Geometry.Point3d":
                    return new Grasshopper.Kernel.Types.GH_Point(JsonConvert.DeserializeObject<Point3d>(data));
                case "Rhino.Geometry.Vector3d":
                    return new Grasshopper.Kernel.Types.GH_Vector(JsonConvert.DeserializeObject<Vector3d>(data));
                case "Rhino.Geometry.Brep":
                case "Rhino.Geometry.Curve":
                case "Rhino.Geometry.Extrusion":
                case "Rhino.Geometry.Mesh":
                case "Rhino.Geometry.PolyCurve":
                case "Rhino.Geometry.NurbsCurve":
                case "Rhino.Geometry.PolylineCurve":
                case "Rhino.Geometry.SubD":
                    {
                        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        var geometry = Rhino.Runtime.CommonObject.FromJSON(dict);
                        Surface surface = geometry as Surface;
                        if (surface != null)
                            geometry = surface.ToBrep();
                        if (geometry is Brep)
                            return new Grasshopper.Kernel.Types.GH_Brep(geometry as Brep);
                        if (geometry is Curve)
                            return new Grasshopper.Kernel.Types.GH_Curve(geometry as Curve);
                        if (geometry is Mesh)
                            return new Grasshopper.Kernel.Types.GH_Mesh(geometry as Mesh);
                        if (geometry is SubD)
                            return new Grasshopper.Kernel.Types.GH_SubD(geometry as SubD);
                    }
                    break;
            }
            throw new Exception("unable to convert resthopper data");
        }

        static List<IGH_Param> _params;
        static IGH_Param ParamFromIoResponseSchema(IoParamSchema item)
        {
            if (_params == null)
            {
                _params = new List<IGH_Param>();
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Arc());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Boolean());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Box());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Brep());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Circle());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Colour());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Complex());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Culture());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Curve());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Field());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_FilePath());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_GenericObject());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Geometry());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Group());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Guid());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Integer());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Interval());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Interval2D());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_LatLonLocation());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Line());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Matrix());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Mesh());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_MeshFace());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_MeshParameters());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Number());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Plane());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Point());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Rectangle());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_String());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_StructurePath());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_SubD());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Surface());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Time());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Transform());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Vector());
            }
            foreach(var p in _params)
            {
                if (p.TypeName.Equals(item.ParamType, StringComparison.OrdinalIgnoreCase))
                {
                    var obj = System.Activator.CreateInstance(p.GetType());
                    return obj as IGH_Param;
                }
            }
            return null;
        }

        static void CollectDataHelper<T>(IGH_DataAccess DA, string inputName, bool itemAccess, ref int inputCount, DataTree<ResthopperObject> dataTree)
        {
            if (itemAccess)
            {
                T t = default(T);
                if (DA.GetData(inputName, ref t))
                {
                    inputCount = 1;
                    dataTree.Append(new ResthopperObject(t), "0");
                }
            }
            else
            {
                List<T> list = new List<T>();
                if (DA.GetDataList(inputName, list))
                {
                    inputCount = list.Count;
                    foreach (var item in list)
                    {
                        dataTree.Append(new ResthopperObject(item), "0");
                    }
                }

            }
        }

        public Schema CreateSolveInput(IGH_DataAccess DA, bool cacheSolveOnServer, out List<string> warnings)
        {
            warnings = new List<string>();
            var schema = new Resthopper.IO.Schema();

            schema.CacheSolve = cacheSolveOnServer;
            var inputs = GetInputParams();
            foreach (var kv in inputs)
            {
                var (input, param) = kv.Value;
                string inputName = kv.Key;
                string computeName = input.Name;
                bool itemAccess = input.AtLeast == 1 && input.AtMost == 1;

                var dataTree = new DataTree<Resthopper.IO.ResthopperObject>();
                dataTree.ParamName = computeName;
                schema.Values.Add(dataTree);
                int inputListCount = 0;
                switch (param)
                {
                    case Grasshopper.Kernel.Parameters.Param_Arc _:
                        CollectDataHelper<Arc>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Boolean _:
                        CollectDataHelper<bool>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Box _:
                        CollectDataHelper<Box>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Brep _:
                        CollectDataHelper<Brep>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Circle _:
                        CollectDataHelper<Circle>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Colour _:
                        CollectDataHelper<System.Drawing.Color>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Complex _:
                        CollectDataHelper<Grasshopper.Kernel.Types.Complex>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Culture _:
                        CollectDataHelper<System.Globalization.CultureInfo>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Curve _:
                        CollectDataHelper<Curve>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Field _:
                        CollectDataHelper<Grasshopper.Kernel.Types.GH_Field>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_FilePath _:
                        CollectDataHelper<string>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                        throw new Exception("generic param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Geometry _:
                        CollectDataHelper<GeometryBase>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Group _:
                        throw new Exception("group param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Guid _:
                        throw new Exception("guid param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Integer _:
                        CollectDataHelper<int>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval _:
                        CollectDataHelper<Grasshopper.Kernel.Types.GH_Interval>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                        CollectDataHelper<Grasshopper.Kernel.Types.GH_Interval2D>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                        throw new Exception("latlonlocation param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Line _:
                        CollectDataHelper<Line>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Matrix _:
                        CollectDataHelper<Matrix>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Mesh _:
                        CollectDataHelper<Mesh>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                        CollectDataHelper<MeshFace>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                        throw new Exception("meshparameters param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Number _:
                        CollectDataHelper<double>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                    case Grasshopper.Kernel.Parameters.Param_Plane _:
                        CollectDataHelper<Plane>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Point _:
                        CollectDataHelper<Point3d>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                        CollectDataHelper<Rectangle3d>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                    case Grasshopper.Kernel.Parameters.Param_String _:
                        CollectDataHelper<string>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                        CollectDataHelper<Grasshopper.Kernel.Types.GH_StructurePath>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Surface _:
                        CollectDataHelper<Surface>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Time _:
                        CollectDataHelper<DateTime>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Transform _:
                        CollectDataHelper<Transform>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Vector _:
                        CollectDataHelper<Vector3d>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                    case Grasshopper.Kernel.Special.GH_NumberSlider _:
                        CollectDataHelper<double>(DA, inputName, itemAccess, ref inputListCount, dataTree);
                        break;
                }

                if (!itemAccess)
                {
                    if (inputListCount < input.AtLeast)
                        warnings.Add($"{input.Name} requires at least {input.AtLeast} items");
                    if (inputListCount > input.AtMost)
                        warnings.Add($"{input.Name} requires at most {input.AtMost} items");
                }
            }

            schema.Pointer = Path;

            var pathType = GetPathType();
            if (pathType == PathType.Server)
            {
                string definition = Path.Substring(Path.LastIndexOf('/') + 1);
                schema.Pointer = definition;
            }
            return schema;
        }
    }


    static class RemoteDefinitionCache
    {
        static List<RemoteDefinition> _definitions = new List<RemoteDefinition>();
        static Dictionary<string, FileSystemWatcher> _filewatchers;
        static HashSet<string> _watchedFiles = new HashSet<string>();

        public static void Add(RemoteDefinition definition)
        {
            // we are only interested in caching definitions which reference
            // gh/ghx files so we can use file watchers to make sure everything
            // is in sync
            if (definition.Path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return;
            if (!File.Exists(definition.Path))
                return;
            if (_definitions.Contains(definition))
                return;
            _definitions.Add(definition);
            RegisterFileWatcher(definition.Path);
        }

        public static void Remove(RemoteDefinition definition)
        {
            if (_definitions.Remove(definition))
            {
                string path = Path.GetFullPath(definition.Path);
                string directory = Path.GetDirectoryName(path);
                bool removeFileWatcher = true;
                foreach(var existingDefinition in _definitions)
                {
                    string existingDefPath = Path.GetFullPath(existingDefinition.Path);
                    string existingDefDirectory = Path.GetDirectoryName(existingDefPath);
                    if (directory.Equals(existingDefDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        removeFileWatcher = false;
                        break;
                    }    
                }
                if (removeFileWatcher)
                {
                    if (_filewatchers.TryGetValue(directory, out FileSystemWatcher watcher))
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Dispose();
                        _filewatchers.Remove(directory);
                    }
                }
            }
        }

        static void RegisterFileWatcher(string path)
        {
            if (!File.Exists(path))
                return;

            if (_filewatchers == null)
            {
                _filewatchers = new Dictionary<string, FileSystemWatcher>();
            }

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
            {
                foreach(var definition in _definitions)
                {
                    string definitionPath = Path.GetFullPath(definition.Path);
                    if( path.Equals(definitionPath, StringComparison.OrdinalIgnoreCase))
                    {
                        definition.OnWatchedFileChanged();
                    }
                }

            }
        }


    }
}
