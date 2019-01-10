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

    public class RhinoModule : Nancy.NancyModule
    {
        private IEnumerable<GeometryEndPoint> CreateEndpoints(Assembly assembly, string nameSpace)
        {
            foreach (var export in assembly.GetExportedTypes())
            {
                if (!string.Equals(export.Namespace, nameSpace, StringComparison.Ordinal))
                    continue;
                if (export.IsInterface || export.IsEnum)
                    continue;
                if (export.IsClass || export.IsValueType)
                {
                    var endpoints = GeometryEndPoint.Create(export);
                    foreach (var endpoint in endpoints)
                    {
                        yield return endpoint;
                    }
                }
            }
        }

        public RhinoModule(IRouteCacheProvider routeCacheProvider)
        {
            Get[""] = _ => FixedEndpoints.HomePage(Context);
            Get["/healthcheck"] = _ => "healthy";
            Get["version"] = _ => FixedEndpoints.GetVersion(Context);
            Get["sdk/csharp"] = _ => FixedEndpoints.CSharpSdk(Context);
            Post["hammertime"] = _ => FixedEndpoints.HammerTime(Context);
            Post["/grasshopper"] = _ => ResthopperEndpoints.Grasshopper(Context);
            Post["/io"] = _ => ResthopperEndpoints.GetIoNames(Context);

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

            foreach (string nameSpace in new List<string>() {"Rhino.Geometry", "Rhino.Geometry.Intersect"})
            {
                foreach (var endpoint in CreateEndpoints(typeof(Rhino.RhinoApp).Assembly, nameSpace))
                {
                    string key = endpoint.Path.ToLowerInvariant();
                    Get[key] = _ => endpoint.Get(Context);
                    Post[key] = _ =>
                    {
                        var r = endpoint.Post(Context);
                        r.ContentType = "application/json";
                        return r;
                    };
                }
            }

            //var script = Rhino.Runtime.PythonScript.Create();
            //if( script != null )
            //{
            //    foreach( var endpoint in GeometryEndPoint.Create(typeof(Python)) )
            //    {
            //        string key = endpoint.Path.ToLowerInvariant();
            //        Get[key] = _ => endpoint.Get(Context);
            //        Post[key] = _ => endpoint.Post(Context);

            //    }
            //}

        }
    }

    // TODO Make ArchibaleDictionary serializable
    //class Python
    //{
    //    public static Rhino.Collections.ArchivableDictionary Evaluate(string script,
    //        Rhino.Collections.ArchivableDictionary input)
    //    {
    //        var py = Rhino.Runtime.PythonScript.Create();
    //        py.SetVariable("inputGeometry", inputGeometry);
    //        py.SetVariable("inputPoints", inputPoints);
    //        py.ExecuteScript(script);
    //        outputGeometry = null;
    //        outputPoints = null;
    //        return "done";
    //    }
    //}
}
