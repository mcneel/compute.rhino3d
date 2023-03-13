﻿using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Newtonsoft.Json;
using Resthopper.IO;
using System.IO;
using System.Reflection;
using System.Net.Http;
using Grasshopper.Kernel.Data;

namespace Hops
{
    /// <summary>
    /// RemoteDefinition represents a specific "definition" or "function" that hops will call.
    /// </summary>
    class RemoteDefinition : IDisposable
    {
        /// <summary>
        /// A path string can represent a path to a specific file, a URL for an endpoint on
        /// a hops compatible server, or a Guid representing a single GH component
        /// </summary>
        public enum PathType
        {
            GrasshopperDefinition,
            InternalizedDefinition,
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
        public byte[] _internalizedDefinition = null;
        const string _apiKeyName = "RhinoComputeKey";
        public PathType? _pathType;
        public static RemoteDefinition Create(string path, HopsComponent parentComponent)
        {
            var rc = new RemoteDefinition(path, parentComponent);
            if(path != null)
                RemoteDefinitionCache.Add(rc);
            return rc;
        }

        public void InternalizeDefinition(string path)
        {
            _internalizedDefinition = System.IO.File.ReadAllBytes(path);
            _pathType = PathType.InternalizedDefinition;
            RemoteDefinitionCache.Remove(this);
            _path = null;
        }

        private RemoteDefinition(string path, HopsComponent parentComponent)
        {
            _parentComponent = parentComponent;
            _path = path;
            _internalizedDefinition = null;
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
                _pathType = GetPathType(_path);
            }
            return _pathType.Value;
        }

        public static PathType GetPathType(string path)
        { 
            if (Guid.TryParse(path, out Guid id))
                return PathType.ComponentGuid;
           
            PathType rc = PathType.GrasshopperDefinition;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var getTask = HttpClient.GetAsync(path);
                    var response = getTask.Result;
                    string mediaType = response.Content.Headers.ContentType.MediaType.ToLowerInvariant();
                    if (mediaType.Contains("json"))
                        rc = PathType.Server;
                }
                catch (Exception)
                {
                    rc = PathType.NonresponsiveUrl;
                }
            }
            return rc;
        }

        public string Path { get { return _path; } set { _path = value; } }
        public byte[] InternalizedDefinition { get { return _internalizedDefinition; } set { _internalizedDefinition = value; } }

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

        public void GetRemoteDescription()
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
                case PathType.InternalizedDefinition:
                    address = "internalized";
                    performPost = true;
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
                if (pathType != PathType.InternalizedDefinition)
                {
                    if(Path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        schema.Pointer = address;
                    }
                    else
                    {
                        var bytes = System.IO.File.ReadAllBytes(address);
                        schema.Algo = Convert.ToBase64String(bytes);
                    }
                }
                else
                {
                    if(_internalizedDefinition != null)
                        schema.Algo = Convert.ToBase64String(_internalizedDefinition);
                }
                schema.AbsoluteTolerance = GetDocumentTolerance();
                schema.AngleTolerance = GetDocumentAngleTolerance();
                schema.ModelUnits = GetDocumentUnits();
                string inputJson = JsonConvert.SerializeObject(schema);
                string requestContent = "{";
                requestContent += "\"URL\": \"" + postUrl + "\"," + Environment.NewLine;
                requestContent += "\"Method\": \"POST" + "\"," + Environment.NewLine;
                requestContent += "\"Content\": " + inputJson  + Environment.NewLine;
                requestContent += "}";
                _parentComponent.HTTPRecord.IORequest = requestContent;
                var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                if(!String.IsNullOrEmpty(HopsAppSettings.APIKey))
                    client.DefaultRequestHeaders.Add(_apiKeyName, HopsAppSettings.APIKey);
                if(HopsAppSettings.HTTPTimeout > 0)
                    client.Timeout = TimeSpan.FromSeconds(HopsAppSettings.HTTPTimeout);
                responseTask = client.PostAsync(postUrl, content);
                _parentComponent.HTTPRecord.Schema = schema;
                contentToDispose = content;
            }
            else
            {
                string requestContent = "{";
                requestContent += "\"URL\": \"" + address + "\"," + Environment.NewLine;
                requestContent += "\"Method\": \"GET" + "\"" + Environment.NewLine;
                requestContent += "}";
                _parentComponent.HTTPRecord.IORequest = requestContent;
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
                    _parentComponent.HTTPRecord.IOResponse = "Invalid URL";
                }
                else
                {
                    _parentComponent.HTTPRecord.IOResponse = stringResult;
                    responseSchema = JsonConvert.DeserializeObject<Resthopper.IO.IoResponseSchema>(stringResult);
                    _cacheKey = responseSchema.CacheKey;
                    _parentComponent.HTTPRecord.IOResponseSchema = responseSchema;
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
                _parentComponent.HTTPRecord.IOResponseSchema = responseSchema;
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

        private string GetDocumentUnits()
        {
            var rhinoDoc = Rhino.RhinoDoc.ActiveDoc;
            if (rhinoDoc != null)  //if the rhino document exists, then return the current document units
                return rhinoDoc.ModelUnitSystem.ToString();
            else
            {
                //rhino document is null
                var utilityType = typeof(Grasshopper.Utility);
                if (utilityType == null)
                    return "";  //utility class cannot be found, return nothing
                var method = utilityType.GetMethod("DocumentUnits", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                    return "";  //method cannot be found, return zero
                else
                    return (string)method.Invoke(null, null);  //method exists so call function to get current model units
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

            if (pathType == PathType.GrasshopperDefinition || pathType == PathType.ComponentGuid || pathType == PathType.InternalizedDefinition)
            {
                solveUrl = Servers.GetSolveUrl();
                if (!string.IsNullOrEmpty(_cacheKey))
                    inputSchema.Pointer = _cacheKey;
            }
            else
            {
                int index = Path.LastIndexOf('/');
                var authority = new Uri(Path).Authority;
                solveUrl = "http://" + authority + "/solve";
                //solveUrl = Path.Substring(0, index + 1) + "solve";
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
            string requestContent = "{";
            requestContent += "\"URL\": \"" + solveUrl + "\"," + Environment.NewLine;
            requestContent += "\"content\": " + inputJson + Environment.NewLine;
            requestContent += "}";
            _parentComponent.HTTPRecord.SolveRequest = requestContent;
            using (var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json"))
            {
                HttpClient client = new HttpClient();
                if (!String.IsNullOrEmpty(HopsAppSettings.APIKey))
                    client.DefaultRequestHeaders.Add(_apiKeyName, HopsAppSettings.APIKey);
                if (HopsAppSettings.HTTPTimeout > 0)
                    client.Timeout = TimeSpan.FromSeconds(HopsAppSettings.HTTPTimeout);
                var postTask = client.PostAsync(solveUrl, content);
                var responseMessage = postTask.Result;
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                _parentComponent.HTTPRecord.SolveResponse = stringResult;
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
                        requestContent = "{";
                        requestContent += "\"URL\": \"" + solveUrl + "\"," + Environment.NewLine;
                        requestContent += "\"content\":" + inputJson + Environment.NewLine;
                        requestContent += "}";
                        _parentComponent.HTTPRecord.SolveRequest = requestContent;
                        var content2 = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                        HttpClient client2 = new HttpClient();
                        if (!String.IsNullOrEmpty(HopsAppSettings.APIKey))
                            client2.DefaultRequestHeaders.Add(_apiKeyName, HopsAppSettings.APIKey);
                        if (HopsAppSettings.HTTPTimeout > 0)
                            client2.Timeout = TimeSpan.FromSeconds(HopsAppSettings.HTTPTimeout);
                        postTask = client.PostAsync(solveUrl, content2);
                        responseMessage = postTask.Result;
                        remoteSolvedData = responseMessage.Content;
                        stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                        _parentComponent.HTTPRecord.SolveResponse = stringResult;
                        schema = SafeSchemaDeserialize(stringResult);
                        if (schema == null && responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var badSchema = new Schema();
                            badSchema.Errors.Add("Unable to solve on compute");
                            _parentComponent.HTTPRecord.Schema = badSchema;
                            return badSchema;
                        }
                    }
                    else
                    {
                        if (!fileExists && string.IsNullOrEmpty(inputSchema.Algo) && GetPathType() == PathType.GrasshopperDefinition)
                        {
                            var badSchema = new Schema();
                            badSchema.Errors.Add($"Unable to find file: {Path}");
                            _parentComponent.HTTPRecord.Schema = badSchema;
                            return badSchema;
                        }
                    }
                }

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    var badSchema = new Schema();
                    badSchema.Errors.Add($"Request timeout: {Path}");
                    _parentComponent.HTTPRecord.Schema = badSchema;
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

        public void SetComponentOutputs(Schema schema, IGH_DataAccess DA, List<IGH_Param> outputParams, HopsComponent component)
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

                //Determine if the data coming into any of the inputs is a Data Tree
                bool hasDataTreeAsInput = false;
                foreach(var param in component.Params.Input)
                {
                    if(param.VolatileData.PathCount > 1)
                    {
                        hasDataTreeAsInput = true;
                        break;
                    }
                }

                foreach (var kv in datatree.InnerTree)
                {
                    var tokens = kv.Key.Trim(new char[] { '{', '}' }).Split(';');
                    List<int> elements = new List<int>();
                    if (datatree.InnerTree.Count == 1 && !hasDataTreeAsInput)
                    {
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (i < tokens.Length - 1)
                            {
                                if (!string.IsNullOrWhiteSpace(tokens[i]))
                                    elements.Add(int.Parse(tokens[i]));
                            }
                            else
                                elements.Add(DA.Iteration);
                        }
                    }
                    else
                    {
                        foreach (var token in tokens)
                        {
                            if (!string.IsNullOrWhiteSpace(token))
                                elements.Add(int.Parse(token));
                        }
                    }

                    var path = new Grasshopper.Kernel.Data.GH_Path(elements.ToArray());
                    var localBranch = structure.EnsurePath(path);
                    for (int gooIndex = 0; gooIndex < kv.Value.Count; gooIndex++)
                    {
                        goo = GooFromReshopperObject(kv.Value[gooIndex]);
                        localBranch.Add(goo);
                    }
                }
                if (structure.DataCount == 1)
                    DA.SetData(paramIndex, goo);
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

        static bool CheckMinMax<T>(T item, string name, InputParamSchema schema, ref List<string> errors)
        {
            if (schema.Minimum != null)
            {
                try
                {
                    if (Convert.ToDouble(item) < Convert.ToDouble(schema.Minimum))
                    {
                        errors.Add(String.Format("{0} value must be greater than the specified minimum value of the parameter", name));
                        return false;
                    }
                }
                catch (Exception ex) { 
                    errors.Add(ex.ToString()); 
                    return false; 
                }

            }
            if (schema.Maximum != null)
            {
                try
                {
                    if (Convert.ToDouble(item) > Convert.ToDouble(schema.Maximum))
                    {
                        errors.Add(String.Format("{0} value must be smaller than the specified maximum value of the parameter", name));
                        return false;
                    }
                }
                catch (Exception ex) { 
                    errors.Add(ex.ToString()); 
                    return false; 
                }
            }
            return true;
        }

        static string GetPathFromInputData(IGH_DataAccess DA, HopsComponent component, int paramIndex)
        {
            int pathIndex = 0;
            if (component?.Params.Input[paramIndex].VolatileData?.PathCount > 1)
                pathIndex = DA.Iteration;
            return component?.Params.Input[paramIndex].VolatileData?.Paths[pathIndex].ToString();
        }

        static void CollectDataHelper<T>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree, 
            ref List<string> warnings, 
            ref List<string> errors, 
            bool convertToGeometryBase = false)
        {
            string path = "{0}";
            var paramIndex = component?.Params.IndexOfInputParam(inputName);
            if (paramIndex > -1)
                path = GetPathFromInputData(DA, component, paramIndex.Value);

            if (access == GH_ParamAccess.item)
            {
                T t = default(T);
                if (DA.GetData(inputName, ref t))
                {
                    inputCount = 1;
                    if (convertToGeometryBase)
                    {
                        var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(t);
                        dataTree.Append(new ResthopperObject(gb), path);
                    }
                    else
                    {
                        if(t is double || t is int)
                        {
                            var passed = CheckMinMax<T>(t, inputName, schema, ref errors);
                            if (!passed)
                                return;
                        }
                        dataTree.Append(new ResthopperObject(t), path);
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
                            dataTree.Append(new ResthopperObject(gb), path);
                        }
                        else
                        {
                            if (item is double || item is int)
                            {
                                var passed = CheckMinMax<T>(item, inputName, schema, ref errors);
                                if (!passed)
                                    return;
                            }
                            dataTree.Append(new ResthopperObject(item), path);
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
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree,
            ref List<string> warnings,
            ref List<string> errors) where GHT : GH_Goo<T>
        {
            if (access == GH_ParamAccess.tree)
            {
                string path = "{0}";
                var tree = new Grasshopper.Kernel.Data.GH_Structure<GHT>();
                if (DA.GetDataTree(inputName, out tree))
                {
                    foreach (var treePath in tree.Paths)
                    {
                        path = treePath.ToString();
                        var items = tree[treePath];
                        foreach (var item in items)
                        {
                            if (item is double || item is int)
                            {
                                var passed = CheckMinMax<T>(item.Value, inputName, schema, ref errors);
                                if (!passed)
                                    return;
                            }
                            dataTree.Append(new ResthopperObject(item.Value), path);
                        }
                    }
                }
            }
            else
            {
                CollectDataHelper<T>(DA, component, inputName, schema, access, ref inputCount, dataTree, ref warnings, ref errors);
            }
        }

        static void CollectDataHelperPoints<T>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree,
            ref List<string> warnings,
            ref List<string> errors)
        {
            string path = "{0}";
            var paramIndex = component?.Params.IndexOfInputParam(inputName);
            if (paramIndex > -1)
                path = GetPathFromInputData(DA, component, paramIndex.Value);
            switch (access)
            {
                case GH_ParamAccess.item:
                    GH_Point t = default(GH_Point);
                    if (DA.GetData(inputName, ref t))
                    {
                        inputCount = 1;
                        dataTree.Append(new ResthopperObject(t.Value), path);
                    }
                    break;
                case GH_ParamAccess.list:
                    List<GH_Point> list = new List<GH_Point>();
                    if (DA.GetDataList(inputName, list))
                    {
                        inputCount = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            dataTree.Append(new ResthopperObject(list[i].Value), path);
                        }
                    }
                    break;
                case GH_ParamAccess.tree:
                    var tree = new Grasshopper.Kernel.Data.GH_Structure<GH_Point>();
                    if (DA.GetDataTree(inputName, out tree))
                    {
                        foreach (var treePath in tree.Paths)
                        {
                            path = treePath.ToString();
                            var items = tree[treePath];
                            foreach (var item in items)
                            {
                                dataTree.Append(new ResthopperObject(item.Value), path);
                            }
                        }
                    }
                    break;
            }
        } 

        static void CollectDataHelperGeometryBase<T>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree, 
            ref List<string> warnings, 
            ref List<string> errors)
        {
            string path = "{0}";
            var paramIndex = component?.Params.IndexOfInputParam(inputName);
            if (paramIndex > -1)
                path = GetPathFromInputData(DA, component, paramIndex.Value);
            switch (access)
            {
                case GH_ParamAccess.item:
                    IGH_GeometricGoo t = default(IGH_GeometricGoo);      
                    if (DA.GetData(inputName, ref t))
                    {
                        inputCount = 1;                   
                        var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(t);
                        dataTree.Append(new ResthopperObject(gb), path);
                    }
                    break;
                case GH_ParamAccess.list:
                    List<IGH_GeometricGoo> list = new List<IGH_GeometricGoo>();
                    if (DA.GetDataList(inputName, list))
                    {
                        inputCount = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {  
                            var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(list[i]);
                            dataTree.Append(new ResthopperObject(gb), path);
                        }
                    }
                    break;
                case GH_ParamAccess.tree:
                    var tree = new Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo>();
                    if (DA.GetDataTree(inputName, out tree))
                    {
                        foreach (var treePath in tree.Paths)
                        {
                            path = treePath.ToString();
                            var items = tree[treePath];
                            foreach (var item in items)
                            {
                                var gb = Grasshopper.Kernel.GH_Convert.ToGeometryBase(item);
                                dataTree.Append(new ResthopperObject(gb), path);
                            }
                        }
                    }
                    break;
            }
        }

        internal static GH_ParamAccess AccessFromInput(InputParamSchema input)
        {
            if (input.TreeAccess)
                return GH_ParamAccess.tree;
            else
            {
                if (input.AtLeast == 1 && input.AtMost == 1)
                    return GH_ParamAccess.item;
                if (input.AtLeast == -1 && input.AtMost == -1)
                    return GH_ParamAccess.tree;
                return GH_ParamAccess.list;
            }
        }

        public Schema CreateSolveInput(IGH_DataAccess DA, bool cacheSolveOnServer, int recursionLevel,
            out List<string> warnings, out List<string> errors)
        {
            warnings = new List<string>();
            errors = new List<string>();
            var schema = new Resthopper.IO.Schema();
            schema.RecursionLevel = recursionLevel;
            schema.AbsoluteTolerance = GetDocumentTolerance();
            schema.AngleTolerance = GetDocumentAngleTolerance();
            schema.ModelUnits = GetDocumentUnits();

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
                            CollectDataHelper2<Arc, GH_Arc>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            CollectDataHelper2<bool, GH_Boolean>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            CollectDataHelper2<Box, GH_Box>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            CollectDataHelper2<Brep, GH_Brep>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            CollectDataHelper2<Circle, GH_Circle>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            CollectDataHelper2<System.Drawing.Color, GH_Colour>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            CollectDataHelper2<Complex, GH_ComplexNumber>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            CollectDataHelper2<System.Globalization.CultureInfo, GH_Culture>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            CollectDataHelper2<Curve, GH_Curve>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            CollectDataHelper<Grasshopper.Kernel.Types.GH_Field>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            CollectDataHelper2<string, GH_String>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            CollectDataHelperGeometryBase<IGH_GeometricGoo>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            CollectDataHelper2<Guid, GH_Guid>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            CollectDataHelper2<int, GH_Integer>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            CollectDataHelper2<Interval, GH_Interval>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            CollectDataHelper2<UVInterval, GH_Interval2D>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            CollectDataHelper2<Line, GH_Line>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            CollectDataHelper2<Matrix, GH_Matrix>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            CollectDataHelper2<Mesh, GH_Mesh>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            CollectDataHelper2<MeshFace, GH_MeshFace>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            CollectDataHelper2<MeshingParameters, GH_MeshingParameters>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            CollectDataHelper2<double, GH_Number>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            CollectDataHelper2<Plane, GH_Plane>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            CollectDataHelperPoints<Point3d>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            CollectDataHelper2<Rectangle3d, GH_Rectangle>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            CollectDataHelper2<string, GH_String>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            CollectDataHelper2<Grasshopper.Kernel.Data.GH_Path, GH_StructurePath>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            CollectDataHelper2<SubD, GH_SubD>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            CollectDataHelper<Surface>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            CollectDataHelper2<DateTime, GH_Time>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            CollectDataHelper2<Transform, GH_Transform>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            CollectDataHelper2<Vector3d, GH_Vector>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
                            break;
                        case Grasshopper.Kernel.Special.GH_NumberSlider _:
                            CollectDataHelper2<double, GH_Number>(DA, _parentComponent, inputName, input, access, ref inputListCount, dataTree, ref warnings, ref errors);
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
                var pointer = new Uri(Path).AbsolutePath;
                schema.Pointer = pointer.Substring(1);
            }
            _parentComponent.HTTPRecord.Schema = schema;
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
            if (_definitions.Remove(definition) && definition.Path != null)
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
