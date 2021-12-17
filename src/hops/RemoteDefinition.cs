using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Newtonsoft.Json;
using Resthopper.IO;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net.Http;

namespace Hops
{
    class RemoteDefinition : IDisposable
    {
        enum PathType
        {
            GrasshopperDefinition,
            ComponentGuid,
            Server,
            NonresponsiveUrl,
            InvalidUrl //responding, but does not appear to have anything to do with solving
        }

        HopsComponent _parentComponent;
        Dictionary<string, Tuple<InputParamSchema, IGH_Param>> _inputParams;
        Dictionary<string, IGH_Param> _outputParams;
        string _description = null;
        System.Drawing.Bitmap _customIcon = null;
        string _path = null;
        string _cacheKey = null;
        PathType? _pathType;
        static string _lastIORequest = "";
        static string _lastIOResponse = "";
        static string _lastSolveRequest = "";
        static string _lastSolveResponse = "";

        public static string LastIORequest
        {
            get { return _lastIORequest; }
            set { _lastIORequest = value; }
        }

        public static string LastIOResponse
        {
            get { return _lastIOResponse; }
            set { _lastIOResponse = value; }
        }

        public static string LastSolveRequest
        {
            get { return _lastSolveRequest; }
            set { _lastSolveRequest = value; }
        }

        public static string LastSolveResponse
        {
            get { return _lastSolveResponse; }
            set { _lastSolveResponse = value; }
        }

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

        public bool IsNotResponingUrl()
        {
            var pathtype = GetPathType();
            return pathtype == PathType.NonresponsiveUrl;
        }

        public bool IsInvalidUrl()
        {
            var pathtype = GetPathType();
            return pathtype == PathType.InvalidUrl;
        }

        public void ResetPathType()
        {
            _pathType = null;
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
            System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask;
            IDisposable contentToDispose = null;
            if (performPost)
            {
                string postUrl = Servers.GetDescriptionPostUrl();
                var schema = new Schema();
                if (Path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    schema.Pointer = address;
                }
                else
                {
                    var bytes = System.IO.File.ReadAllBytes(address);
                    schema.Algo = Convert.ToBase64String(bytes);
                }
                string inputJson = JsonConvert.SerializeObject(schema);
                _lastIORequest = "{";
                _lastIORequest += "\"URL\": \"" + postUrl + "\"," + Environment.NewLine;
                _lastIORequest += "\"content\": " + inputJson  + Environment.NewLine;
                _lastIORequest += "}";
                var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                responseTask = HttpClient.PostAsync(postUrl, content);
                contentToDispose = content;
            }
            else
            {
                _lastIORequest = "Address: " + address + Environment.NewLine;
                responseTask = HttpClient.GetAsync(address);
            }

            if (responseTask != null)
            {
                var responseMessage = responseTask.Result;
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(stringResult))
                {
                    _pathType = PathType.InvalidUrl; // Looks like a valid but not related URL
                    _lastIOResponse = "Invalid URL";
                }
                else
                {
                    _lastIOResponse = stringResult;
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
                        // Use reflection until we update requirements for Hops to run on a newer service release of Rhino
                        string svg = responseSchema.Icon;
                        // Check for some hope that the string is svg. Pre-7.7 has a bug where it could crash
                        // Rhino with invalid svg
                        if (svg.IndexOf("svg", StringComparison.InvariantCultureIgnoreCase) < 0 || svg.IndexOf("xmlns", StringComparison.InvariantCultureIgnoreCase) < 0)
                            svg = null;

                        if (svg != null)
                        {
                            var method = typeof(Rhino.UI.DrawingUtilities).GetMethod("BitmapFromSvg");
                            if (method != null)
                            {
                                _customIcon = method.Invoke(null, new object[] { svg, 24, 24 }) as System.Drawing.Bitmap;
                            }
                            //_customIcon = Rhino.UI.DrawingUtilities.BitmapFromSvg(responseSchema.Icon, 24, 24);
                        }
                        if (_customIcon == null)
                        {
                            byte[] bytes = Convert.FromBase64String(responseSchema.Icon);
                            using (var ms = new MemoryStream(bytes))
                            {
                                _customIcon = new System.Drawing.Bitmap(ms);
                                if (_customIcon != null && (_customIcon.Width != 24 || _customIcon.Height != 24))
                                {
                                    // Make sure the custom icon is 24x24 which is what GH expects.
                                    var temp = _customIcon;
                                    _customIcon = new System.Drawing.Bitmap(temp, new System.Drawing.Size(24, 24));
                                    temp.Dispose();
                                }
                            }
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

        private double GetDocumentTolerance()
        {
            var rhinoDoc = Rhino.RhinoDoc.ActiveDoc;
            if (rhinoDoc != null)  //if the rhino document exists, then return the current document tolerance setting
                return rhinoDoc.ModelAbsoluteTolerance;
            else
            {
                //rhino document is null
                var utilityType = typeof(Grasshopper.Utility);
                if (utilityType == null)
                    return 0;  //utility class cannot be found, return zero
                var method = utilityType.GetMethod("DocumentTolerance", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    return 0;  //method cannot be found, return zero
                else
                    return (double)method.Invoke(null, null);  //method exists so call function to get current default tolerance
            }
        }

        private double GetDocumentAngleTolerance()
        {
            var rhinoDoc = Rhino.RhinoDoc.ActiveDoc;
            if (rhinoDoc != null)  //if the rhino document exists, then return the current document tolerance setting in degrees
                return rhinoDoc.ModelAngleToleranceDegrees;
            else
            {
                //rhino document is null
                var utilityType = typeof(Grasshopper.Utility);
                if (utilityType == null)
                    return 0;  //utility class cannot be found, return zero
                var method = utilityType.GetMethod("DocumentAngleTolerance", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    return 0;  //method cannot be found, return zero
                else
                    return (double)method.Invoke(null, null);  //method exists so call function to get current default tolerance
            }
        }

        static System.Net.Http.HttpClient _httpClient = null;
        public static System.Net.Http.HttpClient HttpClient
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

        static Schema SafeSchemaDeserialize(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<Resthopper.IO.Schema>(data);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public Schema Solve(Schema inputSchema, bool useMemoryCache)
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
            if (useMemoryCache && inputSchema.Algo == null)
            {
                var cachedResults = Hops.MemoryCache.Get(inputJson);
                if (cachedResults != null)
                {
                    return cachedResults;
                }
            }

            _lastSolveRequest = "{";
            _lastSolveRequest += "\"URL\": \"" + solveUrl + "\"," + Environment.NewLine;
            _lastSolveRequest += "\"content\": " + inputJson + Environment.NewLine;
            _lastSolveRequest += "}";

            using (var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json"))
            {
                var postTask = HttpClient.PostAsync(solveUrl, content);
                var responseMessage = postTask.Result;
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                _lastSolveResponse = stringResult;
                Schema schema = SafeSchemaDeserialize(stringResult);

                if (schema == null && responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    bool fileExists = File.Exists(Path);
                    if (fileExists && string.IsNullOrEmpty(inputSchema.Algo))
                    {
                        var bytes = System.IO.File.ReadAllBytes(Path);
                        string base64 = Convert.ToBase64String(bytes);
                        inputSchema.Algo = base64;
                        inputJson = JsonConvert.SerializeObject(inputSchema);
                        _lastSolveRequest = "{";
                        _lastSolveRequest += "\"URL\": \"" + solveUrl + "\"," + Environment.NewLine;
                        _lastSolveRequest += "\"content\":" + inputJson + Environment.NewLine;
                        _lastSolveRequest += "}";
                        var content2 = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                        postTask = HttpClient.PostAsync(solveUrl, content2);
                        responseMessage = postTask.Result;
                        remoteSolvedData = responseMessage.Content;
                        stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                        _lastSolveResponse = stringResult;
                        schema = SafeSchemaDeserialize(stringResult);
                        if (schema == null && responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var badSchema = new Schema();
                            badSchema.Errors.Add("Unable to solve on compute");
                            return badSchema;
                        }
                    }
                    else
                    {
                        if (!fileExists && string.IsNullOrEmpty(inputSchema.Algo) && GetPathType() == PathType.GrasshopperDefinition)
                        {
                            var badSchema = new Schema();
                            badSchema.Errors.Add($"Unable to find file: {Path}");
                            return badSchema;
                        }
                    }
                }

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    var badSchema = new Schema();
                    badSchema.Errors.Add($"Request timeout: {Path}");
                    return badSchema;
                }

                bool rebuildDefinition = (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError
                    && schema.Errors.Count > 0
                    && string.Equals(schema.Errors[0], "Bad inputs", StringComparison.OrdinalIgnoreCase));
                if (!rebuildDefinition)
                {
                    if (schema.Values.Count > 0 && schema.Values.Count != _outputParams.Count)
                        rebuildDefinition = true;
                }

                if (rebuildDefinition)
                {
                    GetRemoteDescription();
                    _parentComponent.OnRemoteDefinitionChanged();
                }
                else
                {
                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (useMemoryCache && inputSchema.Algo == null)
                        {
                            Hops.MemoryCache.Set(inputJson, schema);
                        }
                    }
                }
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
                Grasshopper.Kernel.Types.IGH_Goo goo = null;
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
                    var localBranch = structure.EnsurePath(path);
                    for (int gooIndex = 0; gooIndex < kv.Value.Count; gooIndex++)
                    {
                        goo = GooFromReshopperObject(kv.Value[gooIndex]);
                        localBranch.Add(goo);
                        //structure.Insert(goo, path, gooIndex);
                    }
                }
                if (structure.DataCount == 1)
                    DA.SetData(paramIndex, goo);
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

        static IGH_Goo GooFromReshopperObject(ResthopperObject obj)
        {
            if (obj.ResolvedData != null)
                return obj.ResolvedData as Grasshopper.Kernel.Types.IGH_Goo;
            
            string data = obj.Data.Trim('"');
            switch (obj.Type)
            {
                case "System.Boolean":
                    {
                        var boolResult = new Grasshopper.Kernel.Types.GH_Boolean(bool.Parse(data));
                        obj.ResolvedData = boolResult;
                        return boolResult;
                    }
                case "System.Double":
                    {
                        var doubleResult = new Grasshopper.Kernel.Types.GH_Number(double.Parse(data));
                        obj.ResolvedData = doubleResult;
                        return doubleResult;
                    }
                case "System.String":
                    {
                        string unescaped = data;
                        // TODO: This is a a hack. I understand that JSON needs to escape
                        // embedded JSON, but I'm not particularly happy with the following code
                        if (unescaped.Trim().StartsWith("{") && unescaped.Contains("\\"))
                        {
                            unescaped = System.Text.RegularExpressions.Regex.Unescape(data);
                        }
                        var stringResult = new Grasshopper.Kernel.Types.GH_String(unescaped);
                        obj.ResolvedData = stringResult;
                        return stringResult;
                    }
                case "System.Int32":
                    {
                        var intResult = new Grasshopper.Kernel.Types.GH_Integer(int.Parse(data));
                        obj.ResolvedData = intResult;
                        return intResult;
                    }
                case "Rhino.Geometry.Circle":
                    {
                        var circleResult = new Grasshopper.Kernel.Types.GH_Circle(JsonConvert.DeserializeObject<Circle>(data));
                        obj.ResolvedData = circleResult;
                        return circleResult;
                    }
                case "Rhino.Geometry.Line":
                    {
                        var lineResult = new Grasshopper.Kernel.Types.GH_Line(JsonConvert.DeserializeObject<Line>(data));
                        obj.ResolvedData = lineResult;
                        return lineResult;
                    }
                case "Rhino.Geometry.Plane":
                    {
                        var planeResult = new Grasshopper.Kernel.Types.GH_Plane(JsonConvert.DeserializeObject<Plane>(data));
                        obj.ResolvedData = planeResult;
                        return planeResult;
                    }
                case "Rhino.Geometry.Point3d":
                    {
                        var pointResult = new Grasshopper.Kernel.Types.GH_Point(JsonConvert.DeserializeObject<Point3d>(data));
                        obj.ResolvedData = pointResult;
                        return pointResult;
                    }
                case "Rhino.Geometry.Vector3d":
                    {
                        var vectorResult = new Grasshopper.Kernel.Types.GH_Vector(JsonConvert.DeserializeObject<Vector3d>(data));
                        obj.ResolvedData = vectorResult;
                        return vectorResult;
                    }
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
                //FilePath has the same ParamType as String
                //_params.Add(new Grasshopper.Kernel.Parameters.Param_FilePath());
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
                    var ghParam = obj as IGH_Param;
                    if (ghParam!=null)
                    {
                        string name = item.Name;
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (name.StartsWith("RH_IN:"))
                                name = name.Substring("RH_IN:".Length).Trim();
                            if (name.StartsWith("RH_OUT:"))
                                name = name.Substring("RH_OUT:".Length).Trim();
                        }
                        if (!string.IsNullOrEmpty(name))
                            ghParam.Name = item.Name;
                        string nickname = name;
                        if (!string.IsNullOrEmpty(item.Nickname))
                            nickname = item.Nickname;
                        if (!string.IsNullOrEmpty(nickname))
                            ghParam.NickName = nickname;
                    }
                    return ghParam;
                }
            }
            return null;
        }

        static void CollectDataHelper<T>(IGH_DataAccess DA,
            string inputName,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree, bool convertToGeometryBase = false)
        {
            if (access == GH_ParamAccess.item)
            {
                T t = default(T);
                if (DA.GetData(inputName, ref t))
                {
                    inputCount = 1;
                    if (convertToGeometryBase)
                    {
                        var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(t);
                        dataTree.Append(new ResthopperObject(gb), "0");
                    }
                    else
                    {
                        dataTree.Append(new ResthopperObject(t), "0");
                    }
                }
            }
            else if (access == GH_ParamAccess.list)
            {
                List<T> list = new List<T>();
                if (DA.GetDataList(inputName, list))
                {
                    inputCount = list.Count;
                    foreach (var item in list)
                    {
                        if (convertToGeometryBase)
                        {
                            var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(item);
                            dataTree.Append(new ResthopperObject(gb), "0");
                        }
                        else
                        {
                            dataTree.Append(new ResthopperObject(item), "0");
                        }
                    }
                }

            }
            else if (access == GH_ParamAccess.tree)
            {
                var type = typeof(T);
                throw new Exception($"Tree not currently supported for type: {type}");
            }
        }

        static void CollectDataHelper2<T, GHT>(IGH_DataAccess DA,
            string inputName,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree) where GHT : GH_Goo<T>
        {
            if (access == GH_ParamAccess.tree)
            {
                var tree = new Grasshopper.Kernel.Data.GH_Structure<GHT>();
                if (DA.GetDataTree(inputName, out tree))
                {
                    foreach (var path in tree.Paths)
                    {
                        string pathString = path.ToString();
                        var items = tree[path];
                        foreach (var item in items)
                        {
                            dataTree.Append(new ResthopperObject(item.Value), pathString);
                        }
                    }
                }
            }
            else
            {
                CollectDataHelper<T>(DA, inputName, access, ref inputCount, dataTree);
            }
        }


        internal static GH_ParamAccess AccessFromInput(InputParamSchema input)
        {
            if (input.AtLeast == 1 && input.AtMost == 1)
                return GH_ParamAccess.item;
            if (input.AtLeast == -1 && input.AtMost == -1)
                return GH_ParamAccess.tree;
            return GH_ParamAccess.list;
        }

        public Schema CreateSolveInput(IGH_DataAccess DA, bool cacheSolveOnServer, int recursionLevel,
            out List<string> warnings)
        {
            warnings = new List<string>();
            var schema = new Resthopper.IO.Schema();
            schema.RecursionLevel = recursionLevel;
            schema.AbsoluteTolerance = GetDocumentTolerance();
            schema.AngleTolerance = GetDocumentAngleTolerance();

            schema.CacheSolve = cacheSolveOnServer;
            var inputs = GetInputParams();
            if (inputs != null)
            {
                foreach (var kv in inputs)
                {
                    var (input, param) = kv.Value;
                    string inputName = kv.Key;
                    string computeName = input.Name;
                    GH_ParamAccess access = AccessFromInput(input);

                    var dataTree = new DataTree<Resthopper.IO.ResthopperObject>();
                    dataTree.ParamName = computeName;
                    schema.Values.Add(dataTree);
                    int inputListCount = 0;
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            CollectDataHelper2<Arc, GH_Arc>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            CollectDataHelper2<bool, GH_Boolean>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            CollectDataHelper2<Box, GH_Box>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            CollectDataHelper2<Brep, GH_Brep>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            CollectDataHelper2<Circle, GH_Circle>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            CollectDataHelper2<System.Drawing.Color, GH_Colour>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            CollectDataHelper2<Complex, GH_ComplexNumber>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            CollectDataHelper2<System.Globalization.CultureInfo, GH_Culture>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            CollectDataHelper2<Curve, GH_Curve>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            CollectDataHelper<Grasshopper.Kernel.Types.GH_Field>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            CollectDataHelper2<string, GH_String>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            CollectDataHelper<IGH_GeometricGoo>(DA, inputName, access, ref inputListCount, dataTree, true);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            CollectDataHelper2<Guid, GH_Guid>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            CollectDataHelper2<int, GH_Integer>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            CollectDataHelper2<Interval, GH_Interval>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            CollectDataHelper2<UVInterval, GH_Interval2D>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            CollectDataHelper2<Line, GH_Line>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            CollectDataHelper2<Matrix, GH_Matrix>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            CollectDataHelper2<Mesh, GH_Mesh>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            CollectDataHelper2<MeshFace, GH_MeshFace>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            CollectDataHelper2<MeshingParameters, GH_MeshingParameters>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            CollectDataHelper2<double, GH_Number>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            CollectDataHelper2<Plane, GH_Plane>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            // TODO: figure out how Point3d trees should be handled
                            CollectDataHelper<Point3d>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            CollectDataHelper2<Rectangle3d, GH_Rectangle>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            CollectDataHelper2<string, GH_String>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            CollectDataHelper2<Grasshopper.Kernel.Data.GH_Path, GH_StructurePath>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            CollectDataHelper2<SubD, GH_SubD>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            CollectDataHelper<Surface>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            CollectDataHelper2<DateTime, GH_Time>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            CollectDataHelper2<Transform, GH_Transform>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            CollectDataHelper2<Vector3d, GH_Vector>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                        case Grasshopper.Kernel.Special.GH_NumberSlider _:
                            CollectDataHelper2<double, GH_Number>(DA, inputName, access, ref inputListCount, dataTree);
                            break;
                    }

                    if (access == GH_ParamAccess.list)
                    {
                        if (inputListCount < input.AtLeast)
                            warnings.Add($"{input.Name} requires at least {input.AtLeast} items");
                        if (inputListCount > input.AtMost)
                            warnings.Add($"{input.Name} requires at most {input.AtMost} items");
                    }
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
