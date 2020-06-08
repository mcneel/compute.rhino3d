using Microsoft.Owin.Hosting;
using Nancy;
using Nancy.Routing;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Topshelf;

namespace compute.geometry
{
    class Program
    {
        public static IDisposable RhinoCore { get; set; }

        static void Main(string[] args)
        {
            RhinoInside.Resolver.Initialize();
#if DEBUG
            string rhinoSystemDir = @"C:\dev\github\mcneel\rhino\src4\bin\Debug";
            if (File.Exists(rhinoSystemDir + "\\Rhino.exe"))
                RhinoInside.Resolver.RhinoSystemDirectory = rhinoSystemDir;
#endif

            Logging.Init();
            LogVersions();

            Topshelf.HostFactory.Run(x =>
            {
                x.UseSerilog();
                x.ApplyCommandLine();
                x.SetStartTimeout(new TimeSpan(0, 1, 0));
                x.Service<OwinSelfHost>(s =>
                  {
                      s.ConstructUsing(name => new OwinSelfHost());
                      s.WhenStarted(tc => tc.Start());
                      s.WhenStopped(tc => tc.Stop());
                  });
                x.RunAsPrompt();
                x.SetDisplayName("compute.geometry");
            });

            if (RhinoCore != null)
                RhinoCore.Dispose();
        }

        private static void LogVersions()
        {
            string compute_version = null, rhino_version = null;
            try
            {
                compute_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                rhino_version = typeof(Rhino.RhinoApp).Assembly.GetName().Version.ToString();
            }
            catch { }
            Log.Information("Compute {ComputeVersion}, Rhino {RhinoVersion}", compute_version, rhino_version);
        }
    }

    internal class OwinSelfHost
    {
        int _port;
        IDisposable _host;

        public OwinSelfHost()
        {
            _port = Env.GetEnvironmentInt("COMPUTE_BACKEND_PORT", 8081);
        }

        public void Start()
        {
            Log.Information("Launching RhinoCore library as {User}", Environment.UserName);
            Program.RhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            // Debug:   listen on localhost only
            // Release: attempt to listen on 0.0.0.0 (fall back to localhost)

#if DEBUG
            var url = $"http://localhost:{_port}";
#else
            var url = $"http://+:{_port}";
#endif

            StartOptions options = new StartOptions(url);

            // disable built-in owin tracing by using a null traceoutput
            // otherwise we get some of rhino's diagnostic traces showing up
            options.Settings.Add(
                typeof(Microsoft.Owin.Hosting.Tracing.ITraceOutputFactory).FullName,
                typeof(NullTraceOutputFactory).AssemblyQualifiedName);

            try
            {
                _host = WebApp.Start<Startup>(options);
            }
            catch (TargetInvocationException ex)
            {
                // catch exception when urlacl not configured correctly or user not admin
                // TODO: add link to troubleshooting
#if DEBUG
                throw;
#else
                if (!(ex.InnerException is HttpListenerException))
                    throw;

                Log.Warning("Unable to listen on {Url}", url);
                url = $"http://localhost:{_port}";
                Log.Warning("Attempting to listen on {Url} instead...", url);
                options.Urls.Clear();
                options.Urls.Add(url);
                _host = WebApp.Start<Startup>(options);
#endif
            }
            Log.Information("Listening on {Url}", url);
        }

        public void Stop()
        {
            _host.Dispose();
        }
    }

    internal class NullTraceOutputFactory : Microsoft.Owin.Hosting.Tracing.ITraceOutputFactory
    {
        public TextWriter Create(string outputFile)
        {
            return StreamWriter.Null;
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
