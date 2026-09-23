using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Runtime;

namespace compute.geometry
{
    /// <summary>
    /// Script evaluation endpoints backed by Rhino's scripting runtime (Rhino.Runtime.Code),
    /// the same machinery that runs Python 3 and C# script components in Grasshopper.
    ///
    ///   GET  /script/languages   — which languages are available and whether they are warmed up
    ///   POST /script/evaluate    — run a script; "language" is required in the body
    ///   POST /script/python3     — same, language fixed to Python 3 (CPython)
    ///   POST /script/python2     — same, language fixed to Python 2 (IronPython 2.7)
    ///   POST /script/csharp      — same, language fixed to C#
    ///
    /// Request body:
    /// {
    ///   "language":    "python3" | "python2" | "csharp",   // /script/evaluate only
    ///   "script":      "...source text...",
    ///   "inputs":      { "radius": 5.0, "curve": { rhino3dm CommonObject JSON } },
    ///   "outputs":     [ "mesh", "count" ],
    ///   "dataVersion": 8                           // optional 3dm archive version for geometry outputs
    /// }
    ///
    /// Scripts are plain top-level statements. Every input name is a variable that is already
    /// assigned; every output name is a variable the script assigns. Inputs are typed from their
    /// JSON value (number → double or int, string, bool, array → List&lt;T&gt;, CommonObject JSON →
    /// the RhinoCommon type it decodes to), so C# scripts can use them without casts.
    ///
    /// Outputs are untyped (object). Python does not care. In C# do the typed work in locals and
    /// assign the outputs at the end:
    ///
    ///   var m = Mesh.CreateFromSphere(new Sphere(Point3d.Origin, radius), 32, 16);
    ///   mesh = m;               // ok: object
    ///   count = m.Faces.Count;  // ok; "mesh.Faces" would not compile
    ///
    /// Response:
    /// {
    ///   "language":  "python3",
    ///   "outputs":   { "mesh": { rhino3dm CommonObject JSON }, "count": 12 },
    ///   "stdout":    "...",
    ///   "stderr":    "...",
    ///   "elapsedMs": 42
    /// }
    ///
    /// A script that fails to compile or run returns 400 with
    /// { "error": "ScriptError", "type", "message", "traceback", "stdout", "stderr" }.
    ///
    /// These endpoints execute arbitrary code on the server with no sandbox beyond the API key,
    /// so they are disabled unless RHINO_COMPUTE_ENABLE_SCRIPTING=true. Authentication follows the
    /// same rule as /grasshopper: when RHINO_COMPUTE_KEY is configured the ApiKeyMiddleware requires it
    /// on every POST; when it is not, POSTs are open (compute warns loudly at startup). Note that a
    /// Grasshopper definition with a script component posted to /grasshopper is the same capability.
    /// Real isolation belongs at the process level (dedicated user, systemd hardening or a container
    /// with restricted egress).
    ///
    /// Rhino.Runtime.Code is not distributed as a NuGet package, so this module binds to it via
    /// reflection against the assemblies the RhinoCode plug-in loads at startup (see Startup.cs).
    /// </summary>
    public static class ScriptEndpointsModule
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            if (Config.EnableScripting)
            {
                if (string.IsNullOrWhiteSpace(Config.ApiKey))
                    Serilog.Log.Warning("Script endpoints are ENABLED and RHINO_COMPUTE_KEY is not set: this server executes arbitrary Python/C# sent by ANY caller. Set a key before exposing it.");
                else
                    Serilog.Log.Warning("Script endpoints are ENABLED: this server executes arbitrary Python/C# sent by any holder of the API key. Isolate the process.");
            }

            app.MapGet("/script/languages", Languages);
            app.MapPost("/script/evaluate", EvaluateAny);
            app.MapPost("/script/python3", EvaluatePython3);
            app.MapPost("/script/python2", EvaluatePython2);
            app.MapPost("/script/csharp", EvaluateCSharp);
        }

        // Aliases accepted in the "language" field / route, mapped to LanguageSpec property names.
        static readonly Dictionary<string, string> s_languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["python3"] = "Python3", ["python"] = "Python3", ["py"] = "Python3", ["py3"] = "Python3",
            ["python2"] = "Python2", ["py2"] = "Python2", ["ironpython"] = "Python2",
            ["csharp"] = "CSharp", ["cs"] = "CSharp", ["c#"] = "CSharp",
        };

        class ScriptRequest
        {
            [JsonProperty("language")] public string Language { get; set; }
            [JsonProperty("script")] public string Script { get; set; }
            [JsonProperty("inputs")] public Dictionary<string, JToken> Inputs { get; set; }
            [JsonProperty("outputs")] public List<string> Outputs { get; set; }
            [JsonProperty("dataVersion")] public int DataVersion { get; set; }
        }

        /// <summary>
        /// Scripting is on only when explicitly enabled. Authentication is the ApiKeyMiddleware's job,
        /// exactly as for /grasshopper: enforced on every POST when a key is configured, absent otherwise.
        /// </summary>
        static bool ScriptingEnabled(out string reason)
        {
            if (!Config.EnableScripting)
            {
                reason = "Script endpoints are disabled. Set RHINO_COMPUTE_ENABLE_SCRIPTING=true on the server to enable them.";
                return false;
            }
            reason = null;
            return true;
        }

        static Task EvaluateAny(HttpContext ctx) => Evaluate(ctx, null);
        static Task EvaluatePython3(HttpContext ctx) => Evaluate(ctx, "python3");
        static Task EvaluatePython2(HttpContext ctx) => Evaluate(ctx, "python2");
        static Task EvaluateCSharp(HttpContext ctx) => Evaluate(ctx, "csharp");

        static async Task Languages(HttpContext ctx)
        {
            bool enabled = ScriptingEnabled(out string disabledReason);
            var result = new JObject
            {
                ["enabled"] = enabled,
            };
            if (!enabled)
                result["disabledReason"] = disabledReason;
            var languages = new JArray();
            bool ready = RhinoCodeBridge.TryInitialize(out string initError);
            result["runtime"] = ready ? "ready" : "unavailable";
            if (!ready)
                result["runtimeError"] = initError;
            foreach (var name in new[] { "python3", "python2", "csharp" })
            {
                var entry = new JObject { ["name"] = name };
                if (ready)
                    entry["started"] = RhinoCodeBridge.IsLanguageStarted(s_languages[name]);
                languages.Add(entry);
            }
            result["languages"] = languages;
            await WriteJson(ctx, 200, result);
        }

        static async Task Evaluate(HttpContext ctx, string fixedLanguage)
        {
            if (!ScriptingEnabled(out string disabledReason))
            {
                await WriteJson(ctx, 403, new JObject
                {
                    ["error"] = "ScriptingDisabled",
                    ["message"] = disabledReason,
                });
                return;
            }

            using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            ScriptRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<ScriptRequest>(body);
            }
            catch (JsonException ex)
            {
                await WriteJson(ctx, 400, new JObject { ["error"] = "BadRequest", ["message"] = $"Invalid JSON body: {ex.Message}" });
                return;
            }
            if (request == null || string.IsNullOrWhiteSpace(request.Script))
            {
                await WriteJson(ctx, 400, new JObject { ["error"] = "BadRequest", ["message"] = "Body must include a non-empty \"script\"." });
                return;
            }

            string language = fixedLanguage ?? request.Language;
            if (string.IsNullOrWhiteSpace(language) || !s_languages.TryGetValue(language, out string specName))
            {
                await WriteJson(ctx, 400, new JObject
                {
                    ["error"] = "BadRequest",
                    ["message"] = $"Unknown or missing \"language\" ({language ?? "null"}). Use \"python3\", \"python2\" or \"csharp\".",
                });
                return;
            }

            if (!RhinoCodeBridge.TryInitialize(out string initError))
            {
                await WriteJson(ctx, 503, new JObject
                {
                    ["error"] = "ScriptRuntimeUnavailable",
                    ["message"] = $"Rhino scripting runtime is not available: {initError}",
                });
                return;
            }

            int dataVersion = request.DataVersion > 0 ? request.DataVersion : Rhino.RhinoApp.ExeVersion;
            var inputs = new Dictionary<string, object>();
            if (request.Inputs != null)
            {
                foreach (var kv in request.Inputs)
                    inputs[kv.Key] = FromJson(kv.Value);
            }
            var outputNames = request.Outputs ?? new List<string>();

            Serilog.Log.Debug("Received a {Method} request to {Path}: {Language}, {InputCount} inputs, {OutputCount} outputs",
                ctx.Request.Method, ctx.Request.Path, specName, inputs.Count, outputNames.Count);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            RhinoCodeBridge.RunResult run;
            try
            {
                run = RhinoCodeBridge.Run(specName, request.Script, inputs, outputNames);
            }
            catch (MissingMemberException ex)
            {
                // Our reflection binding no longer matches the scripting runtime: a server problem.
                Serilog.Log.Error(ex, "Script endpoint binding error");
                await WriteJson(ctx, 500, new JObject
                {
                    ["error"] = "ScriptRuntimeBindingError",
                    ["message"] = ex.Message,
                });
                return;
            }
            catch (Exception ex)
            {
                // Script-authored failures: compile errors, runtime exceptions, package restore, etc.
                Serilog.Log.Warning("Script ({Language}) failed: {Message}", specName, ex.Message);
                await WriteJson(ctx, 400, new JObject
                {
                    ["error"] = "ScriptError",
                    ["type"] = ex.GetType().Name,
                    ["message"] = ex.Message,
                    // CompileException.ToString() lists diagnostics; ExecuteException.StackTrace carries the
                    // script traceback. ToString() covers both without depending on the concrete types.
                    ["traceback"] = ex.ToString(),
                    ["stdout"] = RhinoCodeBridge.LastStdout,
                    ["stderr"] = RhinoCodeBridge.LastStderr,
                });
                return;
            }
            stopwatch.Stop();

            var outputs = new JObject();
            foreach (var kv in run.Outputs)
                outputs[kv.Key] = ToJson(kv.Value, dataVersion);

            await WriteJson(ctx, 200, new JObject
            {
                ["language"] = specName.ToLowerInvariant(),
                ["outputs"] = outputs,
                ["stdout"] = run.Stdout,
                ["stderr"] = run.Stderr,
                ["elapsedMs"] = stopwatch.ElapsedMilliseconds,
            });
        }

        static async Task WriteJson(HttpContext ctx, int status, JToken json)
        {
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(json.ToString(Formatting.None));
        }

        #region JSON <-> CLR marshalling

        /// <summary>Decode a request input into the CLR value handed to the script.</summary>
        static object FromJson(JToken token)
        {
            if (token == null)
                return null;
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Integer:
                    {
                        long l = token.Value<long>();
                        if (l >= int.MinValue && l <= int.MaxValue)
                            return (int)l;
                        return l;
                    }
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Array:
                    return ListFromJson((JArray)token);
                case JTokenType.Object:
                    {
                        var obj = (JObject)token;
                        if (IsCommonObjectJson(obj))
                            return CommonObjectFromJson(obj);
                        var dict = new Dictionary<string, object>();
                        foreach (var prop in obj.Properties())
                            dict[prop.Name] = FromJson(prop.Value);
                        return dict;
                    }
                default:
                    return token.ToString();
            }
        }

        // Homogeneous arrays become typed lists so C# scripts see List<double>, List<string>, ...
        // rather than List<object>. Numbers always widen to double: a mixed [1, 2.5] array is common
        // and an int list is rarely what a geometry script wants.
        static object ListFromJson(JArray array)
        {
            var items = array.Select(FromJson).ToList();
            if (items.Count == 0)
                return new List<object>();
            if (items.All(i => i is int || i is long || i is double))
                return items.Select(i => Convert.ToDouble(i)).ToList();
            if (items.All(i => i is string))
                return items.Cast<string>().ToList();
            if (items.All(i => i is bool))
                return items.Cast<bool>().ToList();
            if (items.All(i => i is Rhino.Geometry.GeometryBase))
                return items.Cast<Rhino.Geometry.GeometryBase>().ToList();
            if (items.All(i => i is List<double>))
                return items.Cast<List<double>>().ToList();
            return items;
        }

        static bool IsCommonObjectJson(JObject obj)
        {
            return obj["archive3dm"] != null && obj["data"] != null && obj["data"].Type == JTokenType.String;
        }

        static object CommonObjectFromJson(JObject obj)
        {
            int archive3dm = (int)obj["archive3dm"];
            int opennurbs = obj["opennurbs"] != null ? (int)obj["opennurbs"] : 0;
            string data = (string)obj["data"];
            return CommonObject.FromBase64String(archive3dm, opennurbs, data);
        }

        /// <summary>Encode a script output for the response.</summary>
        static JToken ToJson(object value, int dataVersion, int depth = 0)
        {
            if (value == null)
                return JValue.CreateNull();
            if (depth > 32)
                return value.ToString();

            switch (value)
            {
                case string s:
                    return s;
                case bool b:
                    return b;
                case int or long or short or byte or sbyte or uint or ulong or ushort:
                    return JToken.FromObject(value);
                case double or float or decimal:
                    return JToken.FromObject(value);
                case Guid g:
                    return g.ToString();
                case CommonObject co:
                    {
                        var options = new Rhino.FileIO.SerializationOptions { RhinoVersion = dataVersion, WriteUserData = true };
                        return JObject.Parse(co.ToJSON(options));
                    }
                case IDictionary dict:
                    {
                        var obj = new JObject();
                        foreach (DictionaryEntry entry in dict)
                            obj[entry.Key?.ToString() ?? "null"] = ToJson(entry.Value, dataVersion, depth + 1);
                        return obj;
                    }
                case IEnumerable enumerable when IsPlainEnumerable(value):
                    {
                        var array = new JArray();
                        foreach (var item in enumerable)
                            array.Add(ToJson(item, dataVersion, depth + 1));
                        return array;
                    }
            }

            // Python objects that RhinoCode did not convert to CLR types (custom classes, generators, ...)
            // are not meaningfully serializable; return their repr rather than enumerating them.
            if (value.GetType().FullName == "Python.Runtime.PyObject")
                return value.ToString();

            // RhinoCommon structs (Point3d, Vector3d, Plane, Interval, ...) and anything else
            // Newtonsoft can handle, using the same settings the geometry endpoints use.
            try
            {
                var serializer = JsonSerializer.Create(GeometryResolver.Settings(dataVersion));
                return JToken.FromObject(value, serializer);
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }

        // Enumerate CLR collections (lists, arrays, RhinoCommon collections) but not types that merely
        // happen to implement IEnumerable, such as Python.NET proxies.
        static bool IsPlainEnumerable(object value)
        {
            var type = value.GetType();
            if (type.IsArray)
                return true;
            string ns = type.Namespace ?? string.Empty;
            return ns.StartsWith("System", StringComparison.Ordinal) || ns.StartsWith("Rhino", StringComparison.Ordinal);
        }

        #endregion

        /// <summary>
        /// Reflection bridge to Rhino.Runtime.Code and RhinoCodePlatform.Rhino3D. Both assemblies are
        /// loaded by the RhinoCode plug-in that Startup.cs loads; neither is available at compile time.
        /// Equivalent to:
        ///
        ///   Registrar.StartScriptingLanguages(LanguageSpec.Python3);
        ///   var ctx = new RunContext { OutputStream = ..., ErrorStream = ..., AutoApplyParams = true };
        ///   ctx.Inputs.Set("radius", 5.0);   // param type is derived from the value (double)
        ///   ctx.Outputs.Set("mesh", null);   // null => untyped param, "object" in C#
        ///   RhinoCode.RunScript(new SourceCode(LanguageSpec.Python3, script), ctx);
        ///   ctx.Outputs.TryGet("mesh", out object mesh);
        ///
        /// With AutoApplyParams the code's parameter list is rebuilt from the context values on each
        /// run (Code.GetParams): a value's runtime type becomes the param type, null becomes
        /// ParamType.Any. Do not store Param objects as values; that types the variable as Param.
        /// </summary>
        static class RhinoCodeBridge
        {
            public class RunResult
            {
                public Dictionary<string, object> Outputs { get; } = new Dictionary<string, object>();
                public string Stdout { get; set; } = string.Empty;
                public string Stderr { get; set; } = string.Empty;
            }

            static readonly object s_lock = new object();
            static bool s_initialized;
            static string s_initError;

            static Type s_rhinoCodeType;     // Rhino.Runtime.Code.RhinoCode (static)
            static Type s_languageSpecType;  // Rhino.Runtime.Code.Languages.LanguageSpec
            static Type s_runContextType;    // Rhino.Runtime.Code.Execution.RunContext
            static Type s_registrarType;     // RhinoCodePlatform.Rhino3D.Registrar (static)

            static MethodInfo s_runScript;         // RhinoCode.RunScript(ICode, RunContext)
            static MethodInfo s_startLanguages;    // Registrar.StartScriptingLanguages(LanguageSpec, bool)
            static MethodInfo s_isLanguageStarted; // Registrar.IsScriptingLanguageStarted(LanguageSpec), if public
            static ConstructorInfo s_runContextCtor; // RunContext(bool defaultOutputStream, bool defaultErrorStream)
            static ConstructorInfo s_sourceCodeCtor; // SourceCode(LanguageSpec, string)

            static readonly HashSet<string> s_startedLanguages = new HashSet<string>();

            // Captured for error responses; scripts run one at a time under s_lock.
            public static string LastStdout { get; private set; } = string.Empty;
            public static string LastStderr { get; private set; } = string.Empty;

            public static bool TryInitialize(out string error)
            {
                lock (s_lock)
                {
                    if (s_initialized)
                    {
                        error = s_initError;
                        return s_initError == null;
                    }
                    try
                    {
                        Assembly code = LoadAssembly("Rhino.Runtime.Code");
                        Assembly platform = LoadAssembly("RhinoCodePlatform.Rhino3D");

                        s_rhinoCodeType = FindType(code, "Rhino.Runtime.Code.RhinoCode");
                        s_languageSpecType = FindType(code, "Rhino.Runtime.Code.Languages.LanguageSpec");
                        s_runContextType = FindType(code, "Rhino.Runtime.Code.Execution.RunContext");
                        Type icodeType = FindType(code, "Rhino.Runtime.Code.ICode");
                        Type sourceCodeType = FindType(code, "Rhino.Runtime.Code.Execution.SourceCode");
                        s_registrarType = FindType(platform, "RhinoCodePlatform.Rhino3D.Registrar");

                        s_runScript = Require(s_rhinoCodeType.GetMethod("RunScript", new[] { icodeType, s_runContextType }), "RhinoCode.RunScript(ICode, RunContext)");
                        s_startLanguages = Require(s_registrarType.GetMethod("StartScriptingLanguages", new[] { s_languageSpecType, typeof(bool) }), "Registrar.StartScriptingLanguages(LanguageSpec, bool)");
                        s_isLanguageStarted = s_registrarType.GetMethod("IsScriptingLanguageStarted", new[] { s_languageSpecType }); // optional
                        s_runContextCtor = Require(s_runContextType.GetConstructor(new[] { typeof(bool), typeof(bool) }), "RunContext(bool, bool)");
                        s_sourceCodeCtor = Require(sourceCodeType.GetConstructor(new[] { s_languageSpecType, typeof(string) }), "SourceCode(LanguageSpec, string)");

                        s_initError = null;
                        Serilog.Log.Information("Script endpoints bound to {Assembly} {Version}", code.GetName().Name, code.GetName().Version);
                    }
                    catch (Exception ex)
                    {
                        s_initError = ex.Message;
                        Serilog.Log.Error(ex, "Script endpoints could not bind to the Rhino scripting runtime");
                    }
                    s_initialized = true;
                    error = s_initError;
                    return s_initError == null;
                }
            }

            public static bool IsLanguageStarted(string specName)
            {
                lock (s_lock)
                {
                    if (s_startedLanguages.Contains(specName))
                        return true;
                    if (s_isLanguageStarted == null)
                        return false;
                    try
                    {
                        return (bool)s_isLanguageStarted.Invoke(null, new[] { GetSpec(specName) });
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            public static RunResult Run(string specName, string script, Dictionary<string, object> inputs, IList<string> outputNames)
            {
                lock (s_lock)
                {
                    object spec = GetSpec(specName);
                    EnsureLanguageStarted(specName, spec);

                    object context = s_runContextCtor.Invoke(new object[] { false, false });
                    var stdout = new MemoryStream();
                    var stderr = new MemoryStream();
                    SetProperty(context, "OutputStream", stdout);
                    SetProperty(context, "ErrorStream", stderr);
                    SetProperty(context, "AutoApplyParams", true);   // take Inputs/Outputs from this context
                    SetProperty(context, "RestorePackages", true);   // honour "# r: numpy" style requirements

                    object contextInputs = GetProperty(context, "Inputs");
                    object contextOutputs = GetProperty(context, "Outputs");
                    var setSignature = new[] { typeof(string), typeof(object), typeof(bool), typeof(bool) };
                    MethodInfo setInput = Require(contextInputs.GetType().GetMethod("Set", setSignature), "ContextParams.Set(string, object, bool, bool)");
                    MethodInfo setOutput = Require(contextOutputs.GetType().GetMethod("Set", setSignature), "ContextParams.Set(string, object, bool, bool)");
                    MethodInfo tryGetOutput = Require(contextOutputs.GetType().GetMethod("TryGet", new[] { typeof(string), typeof(object).MakeByRefType() }), "ContextParams.TryGet(string, out object)");

                    foreach (var kv in inputs)
                    {
                        // The param type is derived from the value's runtime type, so C# scripts
                        // see a double/string/Mesh/... and need no casts. Null gives an untyped param.
                        setInput.Invoke(contextInputs, new object[] { kv.Key, kv.Value, true, true });
                    }
                    foreach (var name in outputNames)
                    {
                        // Declare with a null value: an untyped output the script is expected to assign.
                        // (Set(name) alone leaves the value as Missing, which GetParams cannot read.)
                        object initial = inputs.TryGetValue(name, out object inputValue) ? inputValue : null;
                        setOutput.Invoke(contextOutputs, new object[] { name, initial, true, true });
                    }

                    LastStdout = LastStderr = string.Empty;
                    var result = new RunResult();
                    try
                    {
                        object source = s_sourceCodeCtor.Invoke(new[] { spec, script });
                        s_runScript.Invoke(null, new[] { source, context });
                    }
                    catch (TargetInvocationException tie) when (tie.InnerException != null)
                    {
                        LastStdout = ReadStream(stdout);
                        LastStderr = ReadStream(stderr);
                        throw tie.InnerException;
                    }
                    finally
                    {
                        result.Stdout = ReadStream(stdout);
                        result.Stderr = ReadStream(stderr);
                    }

                    foreach (var name in outputNames)
                    {
                        var args = new object[] { name, null };
                        bool found = (bool)tryGetOutput.Invoke(contextOutputs, args);
                        result.Outputs[name] = found ? args[1] : null;
                    }
                    return result;
                }
            }

            static void EnsureLanguageStarted(string specName, object spec)
            {
                if (s_startedLanguages.Contains(specName))
                    return;
                Serilog.Log.Information("Starting scripting language {Language} (first use may take a while)", specName);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    s_startLanguages.Invoke(null, new[] { spec, true });
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }
                s_startedLanguages.Add(specName);
                Serilog.Log.Information("Scripting language {Language} started in {Elapsed} ms", specName, stopwatch.ElapsedMilliseconds);
            }

            static object GetSpec(string specName)
            {
                var prop = Require(s_languageSpecType.GetProperty(specName, BindingFlags.Public | BindingFlags.Static), $"LanguageSpec.{specName}");
                return prop.GetValue(null);
            }

            static Assembly LoadAssembly(string name)
            {
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
                if (loaded != null)
                    return loaded;
                // Not loaded yet (e.g. plug-in load was deferred); let the Rhino.Inside resolver find it.
                return Assembly.Load(name);
            }

            static Type FindType(Assembly assembly, string fullName)
            {
                var type = assembly.GetType(fullName, throwOnError: false);
                if (type == null)
                {
                    // Namespace moved? Fall back to a unique public type with the same simple name.
                    string simpleName = fullName.Substring(fullName.LastIndexOf('.') + 1);
                    var candidates = assembly.GetExportedTypes().Where(t => t.Name == simpleName).ToList();
                    if (candidates.Count == 1)
                        type = candidates[0];
                }
                if (type == null)
                    throw new MissingMemberException($"Type {fullName} not found in {assembly.GetName().Name}");
                return type;
            }

            static T Require<T>(T member, string description) where T : MemberInfo
            {
                if (member == null)
                    throw new MissingMemberException($"{description} not found; the scripting runtime API may have changed");
                return member;
            }

            static void SetProperty(object target, string name, object value)
            {
                var prop = Require(target.GetType().GetProperty(name), $"{target.GetType().Name}.{name}");
                prop.SetValue(target, value);
            }

            static object GetProperty(object target, string name)
            {
                var prop = Require(target.GetType().GetProperty(name), $"{target.GetType().Name}.{name}");
                return prop.GetValue(target);
            }

            static string ReadStream(MemoryStream stream)
            {
                try
                {
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
    }
}
