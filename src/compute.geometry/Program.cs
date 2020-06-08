using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Owin.Hosting;
using Nancy;
using Nancy.Routing;
using Serilog;
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

            int port = Env.GetEnvironmentInt("COMPUTE_BACKEND_PORT", 8081);

            //            Topshelf.HostFactory.Run(x =>
            //            {
            //                x.UseSerilog();
            //                x.ApplyCommandLine();
            //                x.SetStartTimeout(new TimeSpan(0, 1, 0));
            //                x.Service<NancySelfHost>(s =>
            //                  {
            //                      s.ConstructUsing(name => new NancySelfHost());
            //                      s.WhenStarted(tc => tc.Start(port));
            //                      s.WhenStopped(tc => tc.Stop());
            //                  });
            //                x.RunAsPrompt();
            //                //x.RunAsLocalService();
            //                x.SetDisplayName("compute.geometry");
            //                x.SetServiceName("compute.geometry");
            //            });

            var url = $"http://localhost:{port}";

            StartOptions options = new StartOptions(url);

            // disable built-in owin tracing by using a null traceoutput
            options.Settings.Add(
                typeof(Microsoft.Owin.Hosting.Tracing.ITraceOutputFactory).FullName,
                typeof(NullTraceOutputFactory).AssemblyQualifiedName);

            //using (WebApp.Start<Startup>(url))
            using (WebApp.Start<Startup>(options))
            {
                Log.Information("Running on {Url}", url);
                Console.ReadLine();
            }

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

    public class NullTraceOutputFactory : Microsoft.Owin.Hosting.Tracing.ITraceOutputFactory
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
