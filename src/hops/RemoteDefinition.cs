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
using System.Net.Http;
using Grasshopper.Kernel.Data;
using System.Linq;
using System.Diagnostics;

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

        HopsComponent parentComponent;
        Dictionary<string, Tuple<InputParamSchema, IGH_Param>> inputParams;
        Dictionary<string, IGH_Param> outputParams;
        string description = null;
        System.Drawing.Bitmap customIcon = null;
        string path = null;
        string cacheKey = null;
        public byte[] internalizedDefinition = null;
        const string API_KEY_NAME = "RhinoComputeKey";
        public PathType? pathType;
        public string filename = string.Empty;

        public static bool IsWebUrl(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        // Pull the human-readable "message" out of compute's JSON error body
        // ({"error":...,"message":"...","stackTrace":[...]}) so the component shows a clean
        // message instead of the raw JSON + stack trace. Falls back to the trimmed body for
        // non-JSON responses (e.g. a plain-text 401).
        // Hint appended to component-error messages whenever the server returns a 500. We can't
        // positively identify the cause from a 500 alone (could be a license issue, missing
        // dependency, malformed request, etc.) so the hint covers the most common case —
        // server-side Rhino licensing — without overclaiming.
        const string ServerInternalErrorHint =
            "This is usually a server-side issue. Check that the server is reachable and that its Rhino license is valid.";

        static string ExtractServerErrorMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;
            string message = body.Trim();
            // HTML response (e.g. ASP.NET's "An error occurred while starting the application"
            // page when compute.geometry fails to initialize Rhino). The full markup is
            // illegible inside a Grasshopper component error tooltip, so try to pull just the
            // <title> for a one-line summary; fall back to a generic placeholder if there's
            // no title or it's empty.
            if (message.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                message.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                var titleMatch = System.Text.RegularExpressions.Regex.Match(
                    message,
                    @"<title[^>]*>(.*?)</title>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                if (titleMatch.Success && !string.IsNullOrWhiteSpace(titleMatch.Groups[1].Value))
                    return titleMatch.Groups[1].Value.Trim();
                return "Server returned an HTML error page (no JSON body).";
            }
            try
            {
                if (Newtonsoft.Json.Linq.JToken.Parse(body) is Newtonsoft.Json.Linq.JObject obj)
                {
                    var m = obj["message"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(m))
                        message = m;
                }
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // not JSON — keep the raw body
            }
            // compute.geometry's global exception handler prefixes unrecognized exception types
            // with the type name (e.g. "HttpRequestException: ..."); strip it so the text is clean.
            var match = System.Text.RegularExpressions.Regex.Match(message, @"^[A-Za-z0-9_.]+Exception:\s*");
            if (match.Success)
                message = message.Substring(match.Length);
            return message;
        }
        private static bool IsGrasshopperDefinition(string filename)
        {
            if (!String.IsNullOrEmpty(filename))
            {
                var extension = System.IO.Path.GetExtension(filename);
                if (extension == ".gh" || extension == ".ghx")
                    return true;
            }
            return false;
        }

        SchemaDataFormat dataFormat = SchemaDataFormat.Resthopper;

        public static RemoteDefinition Create(string path, HopsComponent parentComponent)
        {
            var rc = new RemoteDefinition(path, parentComponent);
            if (path != null)
                RemoteDefinitionCache.Add(rc);

            var filename = String.Empty;
            if (!IsWebUrl(path))
            {
                filename = System.IO.Path.GetFileName(path);
            } 
            else
            {
                Uri uri = new Uri(path);
                filename = uri.Segments[uri.Segments.Length - 1];
            }
            if (IsGrasshopperDefinition(filename))
            {
                rc.filename = filename;
            }
            return rc;
        }

        // Refuse to read excessively large local files into memory. Hops has historically
        // hit raw File.ReadAllBytes on user-supplied paths, which crashes the entire Rhino
        // process if the user accidentally points at a non-Grasshopper file (a multi-GB log,
        // database dump, etc.). A real Grasshopper definition is well under the default cap;
        // anything larger is almost certainly a misconfiguration. Surfaced cleanly so the
        // caller can show a runtime error on the component instead of taking down Rhino.
        //
        // Override via the HOPS_MAX_LOCAL_FILE_SIZE environment variable (bytes). The value
        // is read once on first access and cached for the lifetime of the process.
        const string HOPS_MAX_LOCAL_FILE_SIZE = "HOPS_MAX_LOCAL_FILE_SIZE";
        const long DefaultMaxLocalFileSize = 100L * 1024 * 1024; // 100 MB
        static long? maxLocalFileSize;
        static long MaxLocalFileSize
        {
            get
            {
                if (!maxLocalFileSize.HasValue)
                {
                    var raw = Environment.GetEnvironmentVariable(HOPS_MAX_LOCAL_FILE_SIZE);
                    if (!string.IsNullOrWhiteSpace(raw) && long.TryParse(raw, out long parsed) && parsed > 0)
                        maxLocalFileSize = parsed;
                    else
                        maxLocalFileSize = DefaultMaxLocalFileSize;
                }
                return maxLocalFileSize.Value;
            }
        }

        static byte[] ReadLocalFileWithSizeCap(string path)
        {
            var info = new System.IO.FileInfo(path);
            long cap = MaxLocalFileSize;
            if (info.Length > cap)
                throw new InvalidOperationException(
                    $"File '{System.IO.Path.GetFileName(path)}' is {info.Length:N0} bytes — exceeds the {cap:N0}-byte cap on local definition files (override via the {HOPS_MAX_LOCAL_FILE_SIZE} environment variable).");
            return System.IO.File.ReadAllBytes(path);
        }

        // Builds the debug envelope captured into HttpRecord.{IoRequest,SolveRequest}.
        // Using a JObject (Newtonsoft is already a dependency) instead of hand-rolled string
        // concat gives a consistent shape across all sites and proper JSON escaping for free.
        // contentJson is embedded as a parsed JToken (not a string) so it nests cleanly.
        static string BuildRequestEnvelope(string url, string method, string contentJson = null)
        {
            var obj = new Newtonsoft.Json.Linq.JObject
            {
                ["URL"] = url,
                ["Method"] = method,
            };
            if (!string.IsNullOrEmpty(contentJson))
                obj["Content"] = Newtonsoft.Json.Linq.JToken.Parse(contentJson);
            return obj.ToString();
        }

        public void InternalizeDefinition(string path)
        {
            internalizedDefinition = ReadLocalFileWithSizeCap(path);
            pathType = PathType.InternalizedDefinition;
            RemoteDefinitionCache.Remove(this);
            this.path = null;
        }

        private RemoteDefinition(string path, HopsComponent parentComponent)
        {
            this.parentComponent = parentComponent;
            this.path = path;
            internalizedDefinition = null;
        }

        public void Dispose()
        {
            parentComponent = null;
            RemoteDefinitionCache.Remove(this);
        }

        public bool IsNotRespondingUrl()
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
            pathType = null;
        }

        PathType GetPathType()
        {
            if (!pathType.HasValue)
            {
                pathType = GetPathType(path);
            }
            return pathType.Value;
        }

        public static PathType GetPathType(string path)
        { 
            if (Guid.TryParse(path, out Guid id))
                return PathType.ComponentGuid;
           
            PathType rc = PathType.GrasshopperDefinition;
            if (IsWebUrl(path))
            {
                try
                {
                    using (var cts = CreateTimeoutCts())
                    {
                        var response = HttpClient.GetAsync(path, cts.Token).Result;
                        string mediaType = response.Content.Headers.ContentType.MediaType.ToLowerInvariant();
                        if (mediaType.Contains("json"))
                            rc = PathType.Server;
                    }
                }
                catch (Exception)
                {
                    rc = PathType.NonresponsiveUrl;
                }
            }
            return rc;
        }

        public string Path { get { return path; } set { path = value; } }
        public byte[] InternalizedDefinition { get { return internalizedDefinition; } set { internalizedDefinition = value; } }

        public void OnWatchedFileChanged()
        {
            cacheKey = null;
            description = null;
            if (parentComponent != null)
                parentComponent.OnRemoteDefinitionChanged();
        }

        public Dictionary<string, Tuple<InputParamSchema, IGH_Param>> GetInputParams()
        {
            if( inputParams == null)
            {
                GetRemoteDescription();
            }
            return inputParams;
        }

        public Dictionary<string, IGH_Param> GetOutputParams()
        {
            if (outputParams == null)
            {
                GetRemoteDescription();
            }
            return outputParams;
        }

        public string GetDescription(out System.Drawing.Bitmap customIcon)
        {
            if (description == null)
            {
                GetRemoteDescription();
            }
            customIcon = this.customIcon;
            return description;
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
                        if (IsWebUrl(Path) || File.Exists(Path))
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
            // Per-request disposables that need to outlive the if/else so the response can be
            // awaited safely. Disposed in a single sweep at the end of the method.
            System.Net.Http.StringContent contentToDispose = null;
            System.Net.Http.HttpRequestMessage requestToDispose = null;
            System.Threading.CancellationTokenSource ctsToDispose = null;
            if (performPost)
            {
                string postUrl = Servers.GetDescriptionPostUrl();
                var schema = new Schema();
                if (pathType != PathType.InternalizedDefinition)
                {
                    if(IsWebUrl(Path))
                    {
                        schema.Pointer = address;
                    }
                    else
                    {
                        if (File.Exists(address))
                        {
                            try
                            {
                                var bytes = ReadLocalFileWithSizeCap(address);
                                schema.Algo = Convert.ToBase64String(bytes);
                            }
                            catch (InvalidOperationException ex)
                            {
                                // Surface the size-cap failure on the component AND skip the
                                // doomed POST — otherwise the server would just see an empty
                                // Algo and return a generic deserialize error, masking the
                                // real cause.
                                HopsLog.Log.Error(ex.Message);
                                var errSchema = new IoResponseSchema();
                                errSchema.Errors.Add(ex.Message);
                                parentComponent.HttpRecord.IoResponseSchema = errSchema;
                                return;
                            }
                        }
                        else
                        {
                            HopsLog.Log.Error($"File not found: {address}");
                        }
                    }
                }
                else
                {
                    if(internalizedDefinition != null)
                        schema.Algo = Convert.ToBase64String(internalizedDefinition);
                }
                schema.AbsoluteTolerance = GetDocumentTolerance();
                schema.AngleTolerance = GetDocumentAngleTolerance();
                schema.ModelUnits = GetDocumentUnits();
                schema.FileName = filename;
                string inputJson = JsonConvert.SerializeObject(schema);
                parentComponent.HttpRecord.IoRequest = BuildRequestEnvelope(postUrl, "POST", inputJson);
                var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, postUrl) { Content = content };
                AddApiKeyHeader(request);
                var cts = CreateTimeoutCts();
                var fileNameMsg = String.Empty;
                if (!String.IsNullOrEmpty(filename))
                    fileNameMsg = $" with {filename}";
                HopsLog.Log.Debug($"Sending POST request to {postUrl}{fileNameMsg}");
                responseTask = HttpClient.SendAsync(request, cts.Token);
                parentComponent.HttpRecord.Schema = schema;
                contentToDispose = content;
                requestToDispose = request;
                ctsToDispose = cts;
            }
            else
            {
                parentComponent.HttpRecord.IoRequest = BuildRequestEnvelope(address, "GET");
                var cts = CreateTimeoutCts();
                responseTask = HttpClient.GetAsync(address, cts.Token);
                ctsToDispose = cts;
            }
            if (responseTask != null)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var responseMessage = responseTask.Result;
                    HopsLog.Log.Debug($"Received response {responseMessage.StatusCode} in {sw.ElapsedMilliseconds}ms");
                    var remoteSolvedData = responseMessage.Content;
                    var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                    parentComponent.HttpRecord.IoResponse = stringResult;

                    // Detect 401 BEFORE attempting to parse the body as JSON. The middleware's
                    // response body is plain text ("Api Key was not provided." / "Unauthorized
                    // client."), which would throw JsonReaderException inside the deserializer
                    // below. Surface via IoResponseSchema.Errors so DefineInputsAndOutputs picks
                    // it up and displays a red runtime message on the Hops component.
                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var serverMessage = string.IsNullOrWhiteSpace(stringResult)
                            ? "Server returned 401 Unauthorized — verify the RHINO_COMPUTE_KEY environment variable on the Hops client matches the server's configured key."
                            : $"Server returned 401 Unauthorized: {stringResult.Trim()}";
                        HopsLog.Log.Error(serverMessage);
                        var errSchema = new IoResponseSchema();
                        errSchema.Errors.Add(serverMessage);
                        parentComponent.HttpRecord.IoResponseSchema = errSchema;
                    }
                    else if (!responseMessage.IsSuccessStatusCode)
                    {
                        // Any other non-success status (500/502/503/etc.) — e.g. the server's
                        // global exception handler returned a JSON error body, or an upstream
                        // URL fetch failed. Deserializing that body into an IoResponseSchema
                        // yields null Inputs/Outputs and would NRE in the foreach loops below,
                        // so surface it as a component error instead.
                        var detail = ExtractServerErrorMessage(stringResult);
                        var serverMessage = string.IsNullOrWhiteSpace(detail)
                            ? $"Could not load the remote definition from {Path}\r\nThe server responded with {(int)responseMessage.StatusCode} ({responseMessage.StatusCode})."
                            : $"Could not load the remote definition from {Path}\r\n{detail}";
                        if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                            serverMessage += "\r\n" + ServerInternalErrorHint;
                        HopsLog.Log.Error(serverMessage);
                        var errSchema = new IoResponseSchema();
                        errSchema.Errors.Add(serverMessage);
                        parentComponent.HttpRecord.IoResponseSchema = errSchema;
                    }
                    else if (string.IsNullOrEmpty(stringResult))
                    {
                        this.pathType = PathType.InvalidUrl; // Looks like a valid but not related URL
                        parentComponent.HttpRecord.IoResponse = "Invalid URL";
                    }
                    else
                    {
                        responseSchema = JsonConvert.DeserializeObject<IoResponseSchema>(stringResult);
                        cacheKey = responseSchema.CacheKey;
                        filename = responseSchema.FileName;
                        parentComponent.HttpRecord.IoResponseSchema = responseSchema;
                        if(responseSchema.SupportedDataFormats != null && responseSchema.SupportedDataFormats.Count > 0)
                        {
                            dataFormat = responseSchema.SupportedDataFormats?.Max() ?? SchemaDataFormat.Resthopper;
                            if (dataFormat > SchemaDataFormat.Grasshopper)
                                dataFormat = SchemaDataFormat.Grasshopper;
                        }
                        else
                        {
                            dataFormat = SchemaDataFormat.Resthopper;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Surface cancellation (HttpTimeout exceeded) or network failure as a clear
                    // runtime message on the component instead of bubbling up to Grasshopper
                    // where the exception is swallowed silently. .Result wraps the underlying
                    // failure in AggregateException; unwrap one level so the message is clean.
                    var inner = ex is AggregateException ae && ae.InnerException != null
                        ? ae.InnerException
                        : ex;
                    string serverMessage;
                    if (inner is OperationCanceledException)
                    {
                        serverMessage = $"Could not load the remote definition from {Path}\r\nRequest timed out after {sw.Elapsed.TotalSeconds:0.0}s (configured timeout: {HopsAppSettings.HttpTimeout}s — raise it in the Hops settings panel if the server is just slow).";
                    }
                    else
                    {
                        serverMessage = $"Could not load the remote definition from {Path}\r\n{inner.Message}";
                    }
                    HopsLog.Log.Error(serverMessage);
                    var errSchema = new IoResponseSchema();
                    errSchema.Errors.Add(serverMessage);
                    parentComponent.HttpRecord.IoResponseSchema = errSchema;
                }
            }

            contentToDispose?.Dispose();
            requestToDispose?.Dispose();
            ctsToDispose?.Dispose();

            if (responseSchema != null)
            { 
                description = responseSchema.Description;
                customIcon = null;
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
                            var method = typeof(Rhino.UI.DrawingUtilities).GetMethod("BitmapFromSvg", new[] {typeof(string), typeof(int), typeof(int)} );
                            if (method != null)
                            {
                                customIcon = method.Invoke(null, new object[] { svg, 24, 24 }) as System.Drawing.Bitmap;
                            }
                            //customIcon = Rhino.UI.DrawingUtilities.BitmapFromSvg(responseSchema.Icon, 24, 24);
                        }
                        if (customIcon == null)
                        {
                            byte[] bytes = Convert.FromBase64String(responseSchema.Icon);
                            using (var ms = new MemoryStream(bytes))
                            {
                                customIcon = new System.Drawing.Bitmap(ms);
                                if (customIcon != null && (customIcon.Width != 24 || customIcon.Height != 24))
                                {
                                    // Make sure the custom icon is 24x24 which is what GH expects.
                                    var temp = customIcon;
                                    customIcon = new System.Drawing.Bitmap(temp, new System.Drawing.Size(24, 24));
                                    temp.Dispose();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HopsLog.Log.Debug(ex, "Failed to parse custom icon for remote definition at {Path}", Path);
                    }
                }

                var inputSuffix = String.Empty;
                var outputSuffix = String.Empty;
                var fileNameMsg = String.Empty;
                if (responseSchema.InputNames.Count > 1)
                    inputSuffix = "s";
                if (responseSchema.OutputNames.Count > 1)
                    outputSuffix = "s";
                if (!String.IsNullOrEmpty(responseSchema.FileName))
                    fileNameMsg = $" in {responseSchema.FileName}";
                HopsLog.Log.Debug($"Compute.Geometry found {responseSchema.InputNames.Count} input{inputSuffix} and {responseSchema.OutputNames.Count} output{outputSuffix}{fileNameMsg}");
                inputParams = new Dictionary<string, Tuple<InputParamSchema, IGH_Param>>();
                outputParams = new Dictionary<string, IGH_Param>();
                // Defensive: a well-formed /io success response always populates these, but
                // guard against null so a partial/unexpected body can't NullReference here.
                foreach (var input in responseSchema.Inputs ?? Enumerable.Empty<InputParamSchema>())
                {
                    string inputParamName = input.Name;
                    if (inputParamName.StartsWith("RH_IN:"))
                    {
                        var chunks = inputParamName.Split(new char[] { ':' });
                        inputParamName = chunks[chunks.Length - 1];
                    }
                    inputParams[inputParamName] = Tuple.Create(input, ParamFromIoResponseSchema(input));
                }
                foreach (var output in responseSchema.Outputs ?? Enumerable.Empty<IoParamSchema>())
                {
                    string outputParamName = output.Name;
                    if (outputParamName.StartsWith("RH_OUT:"))
                    {
                        var chunks = outputParamName.Split(new char[] { ':' });
                        outputParamName = chunks[chunks.Length - 1];
                    }
                    outputParams[outputParamName] = ParamFromIoResponseSchema(output);
                }
                parentComponent.HttpRecord.IoResponseSchema = responseSchema;
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

        static System.Net.Http.HttpClient httpClient = null;
        public static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                {
                    // One shared HttpClient for all requests (avoids per-request socket allocation).
                    // Timeout is intentionally infinite — per-request CancellationTokenSource
                    // controls actual deadlines so HopsAppSettings.HttpTimeout values larger than
                    // 100s aren't capped by HttpClient's default.
                    httpClient = new System.Net.Http.HttpClient
                    {
                        Timeout = System.Threading.Timeout.InfiniteTimeSpan
                    };
                }
                return httpClient;
            }
        }

        // Per-request timeout via CancellationTokenSource. Uses HopsAppSettings.HttpTimeout
        // if configured, otherwise falls back to 100 seconds (HttpClient's historical default).
        static System.Threading.CancellationTokenSource CreateTimeoutCts()
        {
            int seconds = HopsAppSettings.HttpTimeout > 0 ? HopsAppSettings.HttpTimeout : 100;
            return new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        }

        // Adds the RhinoComputeKey header to the per-request HttpRequestMessage when an API
        // key is configured. Headers must go on the request, not the shared HttpClient.
        static void AddApiKeyHeader(System.Net.Http.HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(HopsAppSettings.APIKey))
                request.Headers.Add(API_KEY_NAME, HopsAppSettings.APIKey);
        }

        static Schema SafeSchemaDeserialize(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<Resthopper.IO.Schema>(data);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // Method-name promises "safe" — caller treats null as parse failure and
                // handles it via the bad-schema / status-code paths.
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
                if (!string.IsNullOrEmpty(cacheKey))
                    inputSchema.Pointer = cacheKey;
            }
            else
            {
                var uri = new Uri(Path);
                solveUrl = $"{uri.Scheme}://{uri.Authority}/solve";
            }

            if (!string.IsNullOrEmpty(filename)) inputSchema.FileName = filename;
            string inputJson = JsonConvert.SerializeObject(inputSchema);
            if (useMemoryCache && inputSchema.Algo == null)
            {
                var cachedResults = Hops.MemoryCache.Get(inputJson);
                if (cachedResults != null)
                {
                    return cachedResults;
                }
            }
            string requestContent = BuildRequestEnvelope(solveUrl, "POST", inputJson);
            parentComponent.HttpRecord.SolveRequest = requestContent;
            using (var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json"))
            using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, solveUrl) { Content = content })
            using (var cts = CreateTimeoutCts())
            {
                var sw = Stopwatch.StartNew();
                try
                {
                AddApiKeyHeader(request);
                var postTask = HttpClient.SendAsync(request, cts.Token);
                var fileNameMsg = String.Empty;
                if (!String.IsNullOrEmpty(inputSchema.FileName))
                    fileNameMsg = $" with {inputSchema.FileName} input values";
                HopsLog.Log.Debug($"Sending POST request to {solveUrl}{fileNameMsg}");
                var responseMessage = postTask.Result;
                HopsLog.Log.Debug($"Received response {responseMessage.StatusCode} in {sw.ElapsedMilliseconds}ms");
                var remoteSolvedData = responseMessage.Content;
                var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                parentComponent.HttpRecord.SolveResponse = stringResult;
                Schema schema = SafeSchemaDeserialize(stringResult);
                if (schema == null && responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    bool fileExists = File.Exists(Path);
                    if (fileExists && string.IsNullOrEmpty(inputSchema.Algo))
                    {
                        // Surface this via schema.Warnings rather than AddRuntimeMessage directly.
                        // SetComponentOutputs reads schema.Warnings on the UI thread during the
                        // results-application solve cycle, so it survives the async-mode clear
                        // that BeforeSolveInstance does at the start of that cycle.
                        string autoUploadMessage = $"Server returned HTTP 500. Uploaded local file '{System.IO.Path.GetFileName(Path)}' to {solveUrl} as a fallback.";
                        byte[] bytes;
                        try
                        {
                            bytes = ReadLocalFileWithSizeCap(Path);
                        }
                        catch (InvalidOperationException ex)
                        {
                            var badSchema = new Schema();
                            HopsLog.Log.Error(ex.Message);
                            badSchema.Errors.Add(ex.Message);
                            parentComponent.HttpRecord.Schema = badSchema;
                            return badSchema;
                        }
                        string base64 = Convert.ToBase64String(bytes);
                        inputSchema.Algo = base64;
                        inputSchema.FileName = System.IO.Path.GetFileName(Path);
                        inputJson = JsonConvert.SerializeObject(inputSchema);
                        requestContent = BuildRequestEnvelope(solveUrl, "POST", inputJson);
                        parentComponent.HttpRecord.SolveRequest = requestContent;
                        using var content2 = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
                        using var request2 = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, solveUrl) { Content = content2 };
                        using var cts2 = CreateTimeoutCts();
                        AddApiKeyHeader(request2);
                        postTask = HttpClient.SendAsync(request2, cts2.Token);
                        var fileNameMsg2 = String.Empty;
                        if (!String.IsNullOrEmpty(inputSchema.FileName))
                            fileNameMsg2 = $" with {inputSchema.FileName} input values";
                        HopsLog.Log.Debug($"Sending POST request to {solveUrl}{fileNameMsg2}");
                        var sw2 = Stopwatch.StartNew();
                        responseMessage = postTask.Result;
                        HopsLog.Log.Debug($"Received response {responseMessage.StatusCode} in {sw2.ElapsedMilliseconds}ms");
                        remoteSolvedData = responseMessage.Content;
                        stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                        parentComponent.HttpRecord.SolveResponse = stringResult;
                        schema = SafeSchemaDeserialize(stringResult);
                        if (schema == null && responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var badSchema = new Schema();
                            var errorMsg = "Unable to solve on compute.\r\n" + ServerInternalErrorHint;
                            HopsLog.Log.Error(errorMsg);
                            badSchema.Errors.Add(errorMsg);
                            badSchema.Warnings.Add(autoUploadMessage);
                            parentComponent.HttpRecord.Schema = badSchema;
                            return badSchema;
                        }
                        if (schema != null)
                            schema.Warnings.Add(autoUploadMessage);
                    }
                    else
                    {
                        if (!fileExists && string.IsNullOrEmpty(inputSchema.Algo) && GetPathType() == PathType.GrasshopperDefinition)
                        {
                            var badSchema = new Schema();
                            var errorMsg = $"Unable to find file: {Path}";
                            HopsLog.Log.Error(errorMsg);
                            badSchema.Errors.Add(errorMsg);
                            parentComponent.HttpRecord.Schema = badSchema;
                            return badSchema;
                        }
                    }
                }

                if (responseMessage.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    var badSchema = new Schema();
                    var errorMsg = $"Request timeout: {Path}";
                    HopsLog.Log.Error(errorMsg);
                    badSchema.Errors.Add(errorMsg);
                    parentComponent.HttpRecord.Schema = badSchema;
                    return badSchema;
                }

                // Authentication failure. The same 401 can come from either rhino.compute's
                // ApiKeyMiddleware (when rhino.compute is the front-door) or compute.geometry's
                // ApiKeyMiddleware (when targeted directly), so we don't need to distinguish.
                // Surface it as a component error so the user sees it in Grasshopper rather
                // than getting a silent solve failure.
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var badSchema = new Schema();
                    // Use the server's response body when available — it explains whether the
                    // header was missing or the key was wrong. Fall back to a generic message.
                    var serverMessage = string.IsNullOrWhiteSpace(stringResult)
                        ? "Server returned 401 Unauthorized — verify the RHINO_COMPUTE_KEY environment variable on the Hops client matches the server's configured key."
                        : $"Server returned 401 Unauthorized: {stringResult.Trim()}";
                    HopsLog.Log.Error(serverMessage);
                    badSchema.Errors.Add(serverMessage);
                    parentComponent.HttpRecord.Schema = badSchema;
                    return badSchema;
                }

                bool rebuildDefinition = (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError
                    && schema.Errors.Count > 0
                    && string.Equals(schema.Errors[0], "Bad inputs", StringComparison.OrdinalIgnoreCase));
                if (!rebuildDefinition)
                {
                    if (schema.Values.Count > 0 && schema.Values.Count != outputParams.Count)
                        rebuildDefinition = true;
                }

                if (rebuildDefinition)
                {
                    GetRemoteDescription();
                    parentComponent.OnRemoteDefinitionChanged();
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
                cacheKey = schema.Pointer;
                return schema;
                }
                catch (Exception ex)
                {
                    // Surface cancellation (HttpTimeout exceeded) or network failure (server
                    // unreachable, DNS, connection refused) as a clear runtime message on the
                    // component instead of letting AggregateException bubble up through
                    // SolveInstance — that's been observed to crash Rhino on macOS (RH-86675).
                    // .Result wraps the underlying failure in AggregateException; unwrap one
                    // level so the message is clean.
                    var inner = ex is AggregateException ae && ae.InnerException != null
                        ? ae.InnerException
                        : ex;
                    string serverMessage;
                    if (inner is OperationCanceledException)
                    {
                        serverMessage = $"Could not reach the compute server at {solveUrl}\r\nRequest timed out after {sw.Elapsed.TotalSeconds:0.0}s (configured timeout: {HopsAppSettings.HttpTimeout}s — raise it in the Hops settings panel if the server is just slow).";
                    }
                    else
                    {
                        // Network-level failure (HttpRequestException / SocketException — typically
                        // connection refused, DNS, or TCP-stack timeout from the OS). The TCP-level
                        // timeout will often fire before the configured HttpTimeout, so include
                        // both the actual elapsed time AND the configured timeout so the user
                        // can tell which layer gave up first.
                        serverMessage = $"Could not reach the compute server at {solveUrl}\r\n{inner.Message}\r\nFailed after {sw.Elapsed.TotalSeconds:0.0}s (configured HTTP timeout: {HopsAppSettings.HttpTimeout}s).";
                    }
                    HopsLog.Log.Error(serverMessage);
                    var badSchema = new Schema();
                    badSchema.Errors.Add(serverMessage);
                    parentComponent.HttpRecord.Schema = badSchema;
                    return badSchema;
                }
            }
        }

        public void SetComponentOutputs(Schema schema, IGH_DataAccess DA, List<IGH_Param> outputParams, HopsComponent component)
        {
            if(dataFormat == SchemaDataFormat.Grasshopper)
            {
                foreach (var param in schema.GrasshopperValues.Values)
                {
                    string outputParamName = param.Key;
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
                    DA.SetDataTree(paramIndex, param.Value);
                }
            }
            else if (dataFormat == SchemaDataFormat.Resthopper)
            {
                if (schema.Values.Count > 0)
                {
                    HopsLog.Log.Debug($"Setting output values...");
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
                        foreach (var param in component.Params.Input)
                        {
                            if (param.VolatileData.PathCount > 1)
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
                                goo = GooFromResthopperObject(kv.Value[gooIndex]);
                                localBranch.Add(goo);
                            }
                        }
                        if (structure.DataCount == 1)
                            DA.SetData(paramIndex, goo);
                        else
                            DA.SetDataTree(paramIndex, structure);
                    }
                }
            }

            foreach (var error in schema.Errors)
            {
                component.HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            }
            foreach (var warning in schema.Warnings)
            {
                component.HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }
        }

        // Decode a JSON-encoded string from the wire (the form produced by JsonConvert.SerializeObject
        // for any non-geometry type — surrounded by quote chars with escape sequences for backslashes,
        // embedded quotes, etc.). The primary path uses JsonConvert.DeserializeObject<string> which
        // correctly reverses JSON escapes — so a file path like "C:\\Users\\file.txt" on the wire
        // comes back as "C:\Users\file.txt". Falls back to the legacy embedded-JSON-object handling
        // for malformed input that isn't a valid JSON string literal.
        static string DecodeJsonString(string objData)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(objData);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // Intentional fallback: when objData isn't valid JSON-encoded string syntax,
                // treat it as already-decoded and just strip wrapping quotes / unescape.
                return MaybeUnescapeJsonString(objData.Trim('"'));
            }
        }

        // Legacy fallback for malformed wire payloads that contain an embedded JSON object
        // as a string (e.g. "{\"key\":\"value\"}" with literal backslash-quote pairs that
        // a strict JSON string decoder would reject). Preserves the prior heuristic exactly.
        static string MaybeUnescapeJsonString(string data)
        {
            if (data.Trim().StartsWith("{") && data.Contains("\\"))
                return System.Text.RegularExpressions.Regex.Unescape(data);
            return data;
        }

        static IGH_Goo GooFromResthopperObject(ResthopperObject obj)
        {
            if (obj.ResolvedData != null)
                return obj.ResolvedData as IGH_Goo;

            string data = obj.Data.Trim('"');

            // Simple types: parse or JSON-deserialize and wrap. Each entry is cached on
            // obj.ResolvedData so subsequent calls skip the work.
            IGH_Goo simpleResult = obj.Type switch
            {
                "System.Boolean"             => new GH_Boolean(bool.Parse(data)),
                "System.Double"              => new GH_Number(double.Parse(data)),
                "System.String"              => new GH_String(DecodeJsonString(obj.Data)),
                "System.Int32"               => new GH_Integer(int.Parse(data)),
                "Rhino.Geometry.Circle"      => new GH_Circle(JsonConvert.DeserializeObject<Circle>(data)),
                "Rhino.Geometry.Arc"         => new GH_Arc(JsonConvert.DeserializeObject<Arc>(data)),
                "Rhino.Geometry.Line"        => new GH_Line(JsonConvert.DeserializeObject<Line>(data)),
                "Rhino.Geometry.Rectangle3d" => new GH_Rectangle(JsonConvert.DeserializeObject<Rectangle3d>(data)),
                "Rhino.Geometry.Plane"       => new GH_Plane(JsonConvert.DeserializeObject<Plane>(data)),
                "Rhino.Geometry.Point3d"     => new GH_Point(JsonConvert.DeserializeObject<Point3d>(data)),
                "Rhino.Geometry.Vector3d"    => new GH_Vector(JsonConvert.DeserializeObject<Vector3d>(data)),
                "Rhino.Geometry.Box"         => new GH_Box(JsonConvert.DeserializeObject<Box>(data)),
                _                            => null
            };
            if (simpleResult != null)
            {
                obj.ResolvedData = simpleResult;
                return simpleResult;
            }

            // CommonObject geometry types: deserialize as dictionary, rehydrate via FromJSON,
            // wrap based on runtime type. Original did not cache these on obj.ResolvedData
            // (preserved).
            switch (obj.Type)
            {
                case "Rhino.Geometry.Brep":
                case "Rhino.Geometry.Curve":
                case "Rhino.Geometry.Extrusion":
                case "Rhino.Geometry.Mesh":
                case "Rhino.Geometry.PolyCurve":
                case "Rhino.Geometry.NurbsCurve":
                case "Rhino.Geometry.PolylineCurve":
                case "Rhino.Geometry.SubD":
                case "Rhino.Geometry.PointCloud":
                case "Rhino.Geometry.InstanceReferenceGeometry":
                case "Rhino.Geometry.Hatch":
                case "Rhino.Geometry.LinearDimension":
                case "Rhino.Geometry.AngularDimension":
                case "Rhino.Geometry.RadialDimension":
                case "Rhino.Geometry.OrdinateDimension":
                case "Rhino.Geometry.TextEntity":
                case "Rhino.Geometry.TextDot":
                case "Rhino.Geometry.Leader":
                    var explicitResult = DeserializeExplicitGeometry(data);
                    if (explicitResult != null)
                        return explicitResult;
                    break;
            }

            // Fallback for any Rhino.Geometry.* type not handled above: dynamic type resolution
            // via the assembly that owns Point3d. Surface gets converted to Brep here (different
            // from the explicit case above, which keeps it as GH_Surface) — preserved verbatim.
            if (obj.Type.StartsWith("Rhino.Geometry"))
            {
                var pt = new Rhino.Geometry.Point3d();
                string s = pt.GetType().AssemblyQualifiedName;
                int index = s.IndexOf(",");
                string sType = $"{obj.Type}{s.Substring(index)}";

                System.Type type = System.Type.GetType(sType);
                if (type != null && typeof(GeometryBase).IsAssignableFrom(type))
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                    var geometry = Rhino.Runtime.CommonObject.FromJSON(dict);
                    if (geometry is Surface surface)
                        geometry = surface.ToBrep();
                    IGH_Goo fallbackResult = geometry switch
                    {
                        Brep brep   => new GH_Brep(brep),
                        Curve curve => new GH_Curve(curve),
                        Mesh mesh   => new GH_Mesh(mesh),
                        SubD subD   => new GH_SubD(subD),
                        _           => null
                    };
                    if (fallbackResult != null)
                        return fallbackResult;
                }
            }

            throw new Exception("Unable to convert resthopper data");
        }

        // CommonObject geometry deserializer for the explicit-type cases in GooFromResthopperObject.
        // Order matters: Extrusion : Surface, so Extrusion must come first. A multi-face Brep
        // wraps as GH_Brep; a single-face Brep wraps as GH_Surface (preserved from original).
        static IGH_Goo DeserializeExplicitGeometry(string data)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var geometry = Rhino.Runtime.CommonObject.FromJSON(dict);
            return geometry switch
            {
                Extrusion ext                       => new GH_Extrusion(ext),
                Surface surface                     => new GH_Surface(surface),
                Brep brep when brep.Faces.Count > 1 => new GH_Brep(brep),
                Brep brep                           => new GH_Surface(brep),
                Curve curve                         => new GH_Curve(curve),
                Mesh mesh                           => new GH_Mesh(mesh),
                SubD subD                           => new GH_SubD(subD),
                PointCloud pc                       => new GH_PointCloud(pc),
                InstanceReferenceGeometry iref      => new GH_InstanceReference(iref),
                Hatch hatch                         => new GH_Hatch(hatch),
                LinearDimension lin                 => new GH_LinearDimension(lin),
                AngularDimension ang                => new GH_AngularDimension(ang),
                RadialDimension rad                 => new GH_RadialDimension(rad),
                OrdinateDimension ord               => new GH_OrdinateDimension(ord),
                TextEntity text                     => new GH_TextEntity(text),
                TextDot textDot                     => new GH_TextDot(textDot),
                Leader leader                       => new GH_Leader(leader),
                _                                   => null
            };
        }

        static List<IGH_Param> parameters;
        static IGH_Param ParamFromIoResponseSchema(IoParamSchema item)
        {
            if (parameters == null)
            {
                parameters = new List<IGH_Param>();
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Arc());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Boolean());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Box());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Brep());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Circle());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Colour());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Complex());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Culture());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Curve());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Field());
                //FilePath has the same ParamType as String
                //parameters.Add(new Grasshopper.Kernel.Parameters.Param_FilePath());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_GenericObject());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Geometry());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Group());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Guid());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Integer());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Interval());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Interval2D());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_LatLonLocation());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Line());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Matrix());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Mesh());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_MeshFace());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_MeshParameters());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Number());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Plane());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Point());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Rectangle());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_String());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_StructurePath());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_SubD());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Surface());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Time());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Transform());
                parameters.Add(new Grasshopper.Kernel.Parameters.Param_Vector());
                parameters.Add(new Grasshopper.Rhinoceros.Model.Params.Param_ModelObject());
            }
            foreach(var p in parameters)
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
                    double min = Convert.ToDouble(schema.Minimum);
                    int digits = min.ToString(System.Globalization.CultureInfo.InvariantCulture).SkipWhile(c => c != '.').Skip(1).Count();
                    string formatter = digits < 1 ? "N1" : "N" + digits.ToString();
                    if (Convert.ToDouble(item) < min)
                    {
                        errors.Add($"{name} value must be greater than the specified minimum value ({min.ToString(formatter, System.Globalization.CultureInfo.InvariantCulture)}) of the parameter");
                        return false;
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
                {
                    // Conversion of `item` or `min` to double failed. Surface the short error
                    // message rather than the full ex.ToString() (which includes the stack trace).
                    errors.Add($"{name} value could not be evaluated for the minimum-bound check: {ex.Message}");
                    return false;
                }

            }
            if (schema.Maximum != null)
            {
                try
                {
                    double max = Convert.ToDouble(schema.Maximum);
                    int digits = max.ToString(System.Globalization.CultureInfo.InvariantCulture).SkipWhile(c => c != '.').Skip(1).Count();
                    string formatter = digits < 1 ? "N1" : "N" + digits.ToString();
                    if (Convert.ToDouble(item) > max)
                    {
                        errors.Add($"{name} value must be smaller than the specified maximum value ({max.ToString(formatter, System.Globalization.CultureInfo.InvariantCulture)}) of the parameter");
                        return false;
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
                {
                    // Conversion of `item` or `max` to double failed. Surface the short error
                    // message rather than the full ex.ToString() (which includes the stack trace).
                    errors.Add($"{name} value could not be evaluated for the maximum-bound check: {ex.Message}");
                    return false;
                }
            }
            return true;
        }

        static string GetPathFromInputData(IGH_DataAccess DA, HopsComponent component, int paramIndex)
        {
            int pathIndex = 0;
            var volatileData = component?.Params.Input[paramIndex].VolatileData;
            if (volatileData?.PathCount > 1)
                pathIndex = DA.Iteration;
            if (volatileData?.Paths.Count > 0)
            {
                // When two inputs have different path counts (e.g. Series of 5 vs Range of 10),
                // DA.Iteration can exceed this input's path count once GH starts iterating the
                // longer input. Clamp to the last valid index — matches Grasshopper's longest-list
                // behavior, which DA.GetData already does for the value itself.
                if (pathIndex >= volatileData.Paths.Count)
                    pathIndex = volatileData.Paths.Count - 1;
                return volatileData.Paths[pathIndex].ToString();
            }
            return null;
        }

        static GH_Path GetGHPathFromInputData(IGH_DataAccess DA, HopsComponent component, int paramIndex)
        {
            int pathIndex = 0;
            if (component?.Params.Input[paramIndex].VolatileData?.PathCount > 1)
                pathIndex = DA.Iteration;
            return component?.Params.Input[paramIndex].VolatileData?.Paths[pathIndex];
        }

        static void CollectDataHelper(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            Grasshopper.Kernel.Data.GH_Structure<IGH_Goo> dataTree,
            ref List<string> errors)
        {
            GH_Path path = new GH_Path(0);
            var paramIndex = component?.Params.IndexOfInputParam(inputName);
            if (paramIndex > -1)
                path = GetGHPathFromInputData(DA, component, paramIndex.Value);
            switch (access)
            {
                case GH_ParamAccess.item:
                    IGH_Goo t = default(IGH_Goo);
                    if (DA.GetData(inputName, ref t))
                    {
                        inputCount = 1;
                        dataTree.Append(t, path);
                    }
                    break;
                case GH_ParamAccess.list:
                    List<IGH_Goo> list = new List<IGH_Goo>();
                    if (DA.GetDataList(inputName, list))
                    {
                        inputCount = list.Count;
                        for (int i = 0; i < list.Count; i++)
                        {
                            dataTree.Append(list[i], path);
                        }
                    }
                    break;
                case GH_ParamAccess.tree:
                    if (DA.GetDataTree(inputName, out GH_Structure<IGH_Goo> tree))
                    {
                        foreach (var treePath in tree.Paths)
                        {
                            path = treePath;
                            var items = tree[treePath];
                            foreach (var item in items)
                            {
                                dataTree.Append(item, path);
                            }
                        }
                    }
                    break;
            }
        }

        static void CollectDataHelper<T>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree,
            ref List<string> errors)
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
                    if (t is double || t is int)
                    {
                        var passed = CheckMinMax<T>(t, inputName, schema, ref errors);
                        if (!passed)
                            return;
                    }
                    dataTree.Append(new ResthopperObject(t), path);
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
            else if (access == GH_ParamAccess.tree)
            {
                var type = typeof(T);
                throw new Exception($"Tree not currently supported for type: {type}");
            }
        }

        static void CollectDataHelperWithTree<T, GHT>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree,
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
                            // The original check tested `item is double || item is int`, which is
                            // always false because `item` is a GHT (a reference type) — meaning
                            // Min/Max validation silently never ran on tree-access numerics.
                            // Check `item.Value` (typed as T) so the validation actually fires for
                            // Param_Number and Param_Integer inputs.
                            if (item.Value is double || item.Value is int)
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
                CollectDataHelper<T>(DA, component, inputName, schema, access, ref inputCount, dataTree, ref errors);
            }
        }

        static void CollectDataHelperPoints<T>(IGH_DataAccess DA,
            HopsComponent component,
            string inputName,
            InputParamSchema schema,
            GH_ParamAccess access,
            ref int inputCount,
            DataTree<ResthopperObject> dataTree,
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
            schema.FileName = filename;
            schema.DataFormat = dataFormat;

            schema.CacheSolve = cacheSolveOnServer;
            var inputs = GetInputParams();
            if (inputs != null)
            {
                var msg = String.Empty;
                if (!String.IsNullOrEmpty(schema.FileName))
                    msg = $" for {schema.FileName}";
                HopsLog.Log.Debug($"Collecting input values{msg}...");

                foreach (var kv in inputs)
                {
                    var (input, param) = kv.Value;
                    string inputName = kv.Key;
                    string computeName = input.Name;
                    int inputListCount = 0;
                    GH_ParamAccess access = AccessFromInput(input);
                    if(dataFormat == SchemaDataFormat.Grasshopper)
                    {
                        var goos = new Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>();
                        CollectDataHelper(DA, parentComponent, inputName, input, access, ref inputListCount, goos, ref errors);
                        schema.GrasshopperValues.Values.Add(computeName, goos);
                    }
                    else
                    {
                        var dataTree = new Resthopper.IO.DataTree<Resthopper.IO.ResthopperObject>();
                        dataTree.ParamName = computeName;
                        schema.Values.Add(dataTree);
                        switch (param)
                        {
                            case Grasshopper.Kernel.Parameters.Param_Arc _:
                                CollectDataHelperWithTree<Arc, GH_Arc>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Boolean _:
                                CollectDataHelperWithTree<bool, GH_Boolean>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Box _:
                                CollectDataHelperWithTree<Box, GH_Box>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Brep _:
                                CollectDataHelperWithTree<Brep, GH_Brep>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Circle _:
                                CollectDataHelperWithTree<Circle, GH_Circle>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Colour _:
                                CollectDataHelperWithTree<System.Drawing.Color, GH_Colour>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Complex _:
                                CollectDataHelperWithTree<Complex, GH_ComplexNumber>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Culture _:
                                CollectDataHelperWithTree<System.Globalization.CultureInfo, GH_Culture>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Curve _:
                                CollectDataHelperWithTree<Curve, GH_Curve>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Field _:
                                CollectDataHelper<Grasshopper.Kernel.Types.GH_Field>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_FilePath _:
                                CollectDataHelperWithTree<string, GH_String>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                                throw new Exception("generic objects param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Geometry _:
                                CollectDataHelperGeometryBase<IGH_GeometricGoo>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Group _:
                                throw new Exception("group param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Guid _:
                                CollectDataHelperWithTree<Guid, GH_Guid>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Integer _:
                                CollectDataHelperWithTree<int, GH_Integer>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Interval _:
                                CollectDataHelperWithTree<Interval, GH_Interval>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                                CollectDataHelperWithTree<UVInterval, GH_Interval2D>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                                throw new Exception("latlonlocation param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Line _:
                                CollectDataHelperWithTree<Line, GH_Line>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Matrix _:
                                CollectDataHelperWithTree<Matrix, GH_Matrix>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Mesh _:
                                CollectDataHelperWithTree<Mesh, GH_Mesh>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                                CollectDataHelperWithTree<MeshFace, GH_MeshFace>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                                CollectDataHelperWithTree<MeshingParameters, GH_MeshingParameters>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Number _:
                                CollectDataHelperWithTree<double, GH_Number>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                            case Grasshopper.Kernel.Parameters.Param_Plane _:
                                CollectDataHelperWithTree<Plane, GH_Plane>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Point _:
                                CollectDataHelperPoints<Point3d>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                                CollectDataHelperWithTree<Rectangle3d, GH_Rectangle>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                            case Grasshopper.Kernel.Parameters.Param_String _:
                                CollectDataHelperWithTree<string, GH_String>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                                CollectDataHelperWithTree<Grasshopper.Kernel.Data.GH_Path, GH_StructurePath>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_SubD _:
                                CollectDataHelperWithTree<SubD, GH_SubD>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Surface _:
                                CollectDataHelper<Surface>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Time _:
                                CollectDataHelperWithTree<DateTime, GH_Time>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Transform _:
                                CollectDataHelperWithTree<Transform, GH_Transform>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Vector _:
                                CollectDataHelperWithTree<Vector3d, GH_Vector>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                            case Grasshopper.Kernel.Special.GH_NumberSlider _:
                                CollectDataHelperWithTree<double, GH_Number>(DA, parentComponent, inputName, input, access, ref inputListCount, dataTree, ref errors);
                                break;
                        }
                    }
                    
                    if (access == GH_ParamAccess.list)
                    {
                        if (inputListCount < input.AtLeast)
                        {
                            var atLeastMsg = $"{input.Name} requires at least {input.AtLeast} items";
                            HopsLog.Log.Warning(atLeastMsg);
                            warnings.Add(atLeastMsg);
                        }   
                        if (inputListCount > input.AtMost)
                        {
                            var atMostMsg = $"{input.Name} requires at most {input.AtMost} items";
                            HopsLog.Log.Warning(atMostMsg);
                            warnings.Add(atMostMsg);
                        }
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
            parentComponent.HttpRecord.Schema = schema;
            return schema;
        }
    }


    static class RemoteDefinitionCache
    {
        static List<RemoteDefinition> definitions = new List<RemoteDefinition>();
        static Dictionary<string, FileSystemWatcher> filewatchers;
        static HashSet<string> watchedFiles = new HashSet<string>();

        public static void Add(RemoteDefinition definition)
        {
            // we are only interested in caching definitions which reference
            // gh/ghx files so we can use file watchers to make sure everything
            // is in sync
            if (RemoteDefinition.IsWebUrl(definition.Path))
                return;
            if (!File.Exists(definition.Path))
                return;
            if (definitions.Contains(definition))
                return;
            definitions.Add(definition);
            RegisterFileWatcher(definition.Path);
        }

        public static void Remove(RemoteDefinition definition)
        {
            if (definitions.Remove(definition) && definition.Path != null)
            {
                string path = Path.GetFullPath(definition.Path);
                string directory = Path.GetDirectoryName(path);
                bool removeFileWatcher = true;
                foreach(var existingDefinition in definitions)
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
                    if (filewatchers.TryGetValue(directory, out FileSystemWatcher watcher))
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Dispose();
                        filewatchers.Remove(directory);
                    }
                }
            }
        }

        static void RegisterFileWatcher(string path)
        {
            if (!File.Exists(path))
                return;

            if (filewatchers == null)
            {
                filewatchers = new Dictionary<string, FileSystemWatcher>();
            }

            path = Path.GetFullPath(path);
            if (watchedFiles.Contains(path.ToLowerInvariant()))
                return;

            watchedFiles.Add(path.ToLowerInvariant());
            string directory = Path.GetDirectoryName(path);
            if (filewatchers.ContainsKey(directory) || !Directory.Exists(directory))
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
            filewatchers[directory] = fsw;
        }

        private static void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath.ToLowerInvariant();
            if (watchedFiles.Contains(path))
            {
                foreach(var definition in definitions)
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
