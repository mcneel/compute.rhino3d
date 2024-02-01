using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Runtime.InteropServices;

namespace compute.geometry
{
    class Program
    {
        public static IDisposable RhinoCore { get; set; }
        public static DateTime StartTime { get; set; }

        static void Main(string[] args)
        {
            Config.Load();
            Logging.Init();

            RhinoInside.Resolver.Initialize();
            RhinoInside.Resolver.UseLatest = true;
#if DEBUG
            // Uncomment the following to debug with core Rhino source. This
            // tells compute to use a different RhinoCore than what RhinoInside thinks
            // should use.
            // (for McNeel devs only and only those devs who use the same path as Andy)

            //string rhinoSystemDir = @"C:\dev\github\mcneel\rhino8\src4\bin\Debug";
            //if (System.IO.File.Exists(rhinoSystemDir + "\\Rhino.exe"))
            //    RhinoInside.Resolver.RhinoSystemDirectory = rhinoSystemDir;

#endif
            StartTime = DateTime.Now;
            Shutdown.RegisterStartTime(StartTime);
            Log.Information($"Child process started at " + StartTime.ToLocalTime().ToString());
            ParseCommandLineArgs(args);

            RhinoInside.Resolver.LoadRhino();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var b = webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Handle requests up to 50 MB
                        options.Limits.MaxRequestBodySize = null;//Config.MaxRequestSize;
                        if (Config.LocalhostPort>0)
                            options.ListenLocalhost(Config.LocalhostPort);
                    })
                    //.UseIISIntegration()
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true);

                    //if (port > 0)
                    //{
                    //    b.UseUrls($"http://localhost:{port}");
                    //    ComputeChildren.ParentPort = port;
                    //}

                })
                .UseSerilog(Log.Logger)
                .Build();
            Shutdown.StartTimer(host);
            host.Run();

            if (RhinoCore != null)
                RhinoCore.Dispose();

            Log.CloseAndFlush();
        }

        static void ParseCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string[] items = args[i].Split(':');
                if (items == null || items.Length != 2)
                    continue;
                string key = items[0].ToLowerInvariant().TrimStart('-');
                string value = items[1];
                switch (key)
                {
                    case "port":
                        {
                            Config.LocalhostPort = int.Parse(value);
                            Log.Information($"Parsed port = {Config.LocalhostPort}");
                        }
                        break;
                    case "address":
                        {
                            Config.Urls = new string[] { value };
                        }
                        break;
                    case "childof":
                        {
                            int parentId = int.Parse(value);
                            Shutdown.RegisterParentProcess(parentId);
                        }
                        break;
                    case "parentport":
                        {
                            int parentPort = int.Parse(value);
                            Shutdown.RegisterParentPort(parentPort);
                        }
                        break;
                    case "idlespan":
                        {
                            int span = int.Parse(value);
                            Shutdown.RegisterIdleSpan(span);
                        }
                        break;
                    case "rhinosysdir":
                        RhinoInside.Resolver.RhinoSystemDirectory = value;
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public class RhinoGetModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/sdk", context => SdkEndpoint(context, app));
            
            foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            {
                app.MapGet(endpoint.PathURL, endpoint.Get);
            }
        }

        static async Task SdkEndpoint(HttpContext context, IEndpointRouteBuilder app)
        {
            var result = new StringBuilder("<!DOCTYPE html><html><body>");
            result.AppendLine("<p>API<br>");
            int route_index = 0;
            var sources = app.DataSources;
            var getHeader = "HTTP: GET";
            var postHeader = "HTTP: POST";
            foreach (var source in sources)
            {
                if (source == null) continue;
                foreach (var endpoint in source.Endpoints)  
                {
                    if (endpoint.DisplayName == "Health checks" || endpoint.DisplayName == "HTTP: GET  => HomePage")
                        continue;
                    route_index += 1;
                    var method = endpoint.RequestDelegate;
                    var displayName = endpoint.DisplayName;
                    var path = endpoint.DisplayName;
                    if (path.Contains(getHeader))
                        path = path.Substring(getHeader.Length);
                    else if (path.Contains(postHeader))
                        path = path.Substring(postHeader.Length);

                    path.Trim();
                    result.AppendLine($"{route_index} <a href='{path}'>{displayName}</a><BR>");
                }
            }           
            //foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            //{
            //    route_index += 1;
            //    result.AppendLine($"{route_index} <a href='{endpoint.PathURL}'>{endpoint.Path}</a><BR>");
            //}
            result.AppendLine("</p></body></html>");
            await context.Response.WriteAsync(result.ToString());
        }
    }

    public class RhinoPostModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            {
                app.MapPost(endpoint.PathURL, endpoint.Post);
            }
        }
    }
}
