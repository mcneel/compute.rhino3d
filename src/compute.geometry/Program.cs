using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Extensions;
using Nancy.Hosting.Self;
using Nancy.Routing;
using Nancy.TinyIoc;
using Serilog;
using Topshelf;

namespace compute.geometry
{
    class Program
    {
        static void Main(string[] args)
        {
            Logging.Init();
            int backendPort = Env.GetEnvironmentInt("COMPUTE_BACKEND_PORT", 8081);

            Topshelf.HostFactory.Run(x =>
            {
                x.UseSerilog();
                x.ApplyCommandLine();
                x.SetStartTimeout(new TimeSpan(0, 1, 0));
                x.Service<NancySelfHost>(s =>
                  {
                      s.ConstructUsing(name => new NancySelfHost());
                      s.WhenStarted(tc => tc.Start(backendPort));
                      s.WhenStopped(tc => tc.Stop());
                  });
                x.RunAsPrompt();
                //x.RunAsLocalService();
                x.SetDisplayName("compute.geometry");
                x.SetServiceName("compute.geometry");
            });
            RhinoLib.ExitInProcess();
        }
    }

    public class NancySelfHost
    {
        private NancyHost _nancyHost;
        private System.Diagnostics.Process _backendProcess = null;

        public void Start(int http_port)
        {
            Log.Information("Launching RhinoCore library as {User}", Environment.UserName);
            RhinoLib.LaunchInProcess(RhinoLib.LoadMode.Headless, 0);
            var config = new HostConfiguration();
#if DEBUG
            config.RewriteLocalhost = false;  // Don't require URL registration since geometry service always runs on localhost
#endif
            var listenUriList = new List<Uri>();

            if (http_port > 0)
                listenUriList.Add(new Uri($"http://localhost:{http_port}"));

            if (listenUriList.Count > 0)
                _nancyHost = new NancyHost(config, listenUriList.ToArray());
            else
                Log.Error("Neither COMPUTE_HTTP_PORT nor COMPIUTE_HTTPS_PORT are set. Not listening!");
            try
            {
                _nancyHost.Start();
                foreach (var uri in listenUriList)
                    Log.Information("compute.geometry running on {Uri}", uri.OriginalString);
            }
            catch (AutomaticUrlReservationCreationFailureException)
            {
                Log.Error(GetAutomaticUrlReservationCreationFailureExceptionMessage(listenUriList));
                Environment.Exit(1);
            }
        }

        // TODO: move this somewhere else
        string GetAutomaticUrlReservationCreationFailureExceptionMessage(List<Uri> listenUriList)
        {
            var msg = new StringBuilder();
            msg.AppendLine("Url not reserved. From an elevated command promt, run:");
            msg.AppendLine();
            foreach (var uri in listenUriList)
                msg.AppendLine($"netsh http add urlacl url=\"{uri.Scheme}://+:{uri.Port}/\" user=\"Everyone\"");
            return msg.ToString();
        }

        public void Stop()
        {
            if (_backendProcess != null)
                _backendProcess.Kill();
            _nancyHost.Stop();
        }
    }

    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private byte[] _favicon;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Log.Debug("ApplicationStartup");

            // Load GH at startup so it can get initialized on the main thread
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
            if (runheadless != null)
                runheadless.Invoke(pluginObject, null);

            // Do the same for python
            var script = Rhino.Runtime.PythonScript.Create();

            Nancy.StaticConfiguration.DisableErrorTraces = false;
            pipelines.OnError += (ctx, ex) => LogError(ctx, ex);
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("docs"));
        }

        protected override byte[] FavIcon
        {
            get { return _favicon ?? (_favicon = LoadFavIcon()); }
        }

        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("compute.geometry.favicon.ico"))
            {
                var memoryStream = new System.IO.MemoryStream();
                resourceStream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }

        private static dynamic LogError(NancyContext ctx, Exception ex)
        {
            string id = ctx.Request.Headers["X-Compute-Id"].FirstOrDefault();
            Log.Error(ex, "An exception occured while processing request \"{RequestId}\"", id);
            return null;
        }
    }

    public class RhinoGetModule : NancyModule
    {
        public RhinoGetModule(IRouteCacheProvider routeCacheProvider)
        {
            Get["/sdk"] = _ =>
            {
                var result = new StringBuilder("<!DOCTYPE html><html><body>");
                var cache = routeCacheProvider.GetCache();
                result.AppendLine($" <a href=\"/sdk/csharp\">C# SDK</a><BR>");
                result.AppendLine("<p>API<br>");

                int route_index = 0;
                foreach (var module in cache)
                {
                    foreach (var route in module.Value)
                    {
                        var method = route.Item2.Method;
                        var path = route.Item2.Path;
                        if (method == "GET")
                        {
                            route_index += 1;
                            result.AppendLine($"{route_index} <a href='{path}'>{path}</a><BR>");
                        }
                    }
                }

                result.AppendLine("</p></body></html>");
                return result.ToString();
            };

            foreach(var endpoint in GeometryEndPoint.AllEndPoints)
            {
                string key = endpoint.PathURL;
                Get[key] = _ => endpoint.Get(Context);
            }
        }
    }

    public class RhinoPostModule : NancyModule
    {
        public RhinoPostModule(IRouteCacheProvider routeCacheProvider)
        {
            foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            {
                string key = endpoint.PathURL;
                Post[key] = _ =>
                {
                    var r = endpoint.Post(Context);
                    r.ContentType = "application/json";
                    return r;
                };
            }
        }
    }
}

namespace Rhino
{
    static class Python
    {
        static string _previousScript = "";
        static Runtime.PythonCompiledCode _previousCompile = null;

        public static Rhino.Collections.ArchivableDictionary Evaluate(string script,
            Rhino.Collections.ArchivableDictionary input,
            string[] outputNames)
        {
            var py = Rhino.Runtime.PythonScript.Create();
            foreach(var kv in input)
                py.SetVariable(kv.Key, kv.Value);
            if (!script.Equals(_previousScript))
            {
                // Don't allow certain words in the script to attempt to avoid
                // malicious attacks
                string[] badwords = {"exec", "Assembly", "GetType", "Activator", "GetMethod", "GetPropert" };
                foreach(var word in badwords )
                {
                    if (script.IndexOf(word)>=0)
                        throw new Exception($"Script is not allowed to contain the word {word}");
                }

                // validate that only Rhino namespaces are imported
                const string import = "import ";
                int importIndex = script.IndexOf(import);
                while (importIndex >= 0)
                {
                    importIndex += import.Length;
                    while (importIndex < script.Length)
                    {
                        char c = script[importIndex];
                        if (c == ' ')
                        {
                            importIndex++;
                            continue;
                        }
                        break;
                    }
                    if (script.IndexOf("Rhino", importIndex) != importIndex && script.IndexOf("rhinoscript", importIndex) != importIndex)
                        throw new Exception("Attempt to import module that is not permitted");

                    int commaAndContinuationIndex = script.IndexOfAny(new char[] { ',', '\\' }, importIndex);
                    if (commaAndContinuationIndex > 0)
                    {
                        int newlineIndex = script.IndexOf('\n', importIndex);
                        if (commaAndContinuationIndex < newlineIndex)
                            throw new Exception("Do not import multiple packages with a single import statement");
                    }

                    importIndex = script.IndexOf(import, importIndex);
                }
                _previousCompile = py.Compile(script);
                _previousScript = script;
            }
            _previousCompile.Execute(py);

            var rc = new Rhino.Collections.ArchivableDictionary();
            foreach (var name in outputNames)
                rc[name] = py.GetVariable(name);
            return rc;
        }
    }
}
