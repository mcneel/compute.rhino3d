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
        const string _env_port = "COMPUTE_BACKEND_PORT";
        const string _env_bind = "COMPUTE_BIND_URLS";
        string[] _bind;
        IDisposable _host;

        public OwinSelfHost()
        {
            var str = Env.GetEnvironmentString(_env_bind, null);

            if (!string.IsNullOrEmpty(str))
            {
                _bind = str.Split(';');

                if (Env.GetEnvironmentInt(_env_port, 0) > 0)
                    Log.Warning($"Ignoring deprecated {_env_port} environment variable");
            }

            // fallback to existing behaviour (COMPUTE_BACKEND_PORT)
            // Debug:   listen on localhost only
            // Release: attempt to listen on 0.0.0.0 (fall back to localhost)
            else
            {
                var port = Env.GetEnvironmentInt(_env_port, 8081);
                var url = $"http://localhost:{port}";
                _bind = new string[] { url };
            }
        }

        public void Start()
        {
            Log.Information("Launching RhinoCore library as {User}", Environment.UserName);
            Program.RhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            Rhino.Runtime.HostUtils.OnExceptionReport += (source, ex) => {
                Log.Error(ex, "An exception occured while processing request");
            };

            StartOptions options = new StartOptions();
            foreach (var url in _bind)
            {
                options.Urls.Add(url);
            }

            // disable built-in owin tracing by using a null traceoutput
            // otherwise we get some of rhino's diagnostic traces showing up
            options.Settings.Add(
                typeof(Microsoft.Owin.Hosting.Tracing.ITraceOutputFactory).FullName,
                typeof(NullTraceOutputFactory).AssemblyQualifiedName);

            Log.Information("Starting listener(s): {Urls}", _bind);

            // start listener and unpack HttpListenerException if thrown
            // (missing urlacl or lack of permissions)
            try
            {
                _host = WebApp.Start<Startup>(options);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is HttpListenerException hle)
                    throw hle; // TODO: add link to troubleshooting

                throw ex;
            }
            catch
            {
                throw;
            }

            Log.Information("Listening on {Urls}", _bind);
        }

        public void Stop()
        {
            _host?.Dispose();
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
