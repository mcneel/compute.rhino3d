using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.IO;


namespace compute.geometry
{
    class Program
    {
        public static IDisposable RhinoCore { get; set; }
        public static DateTime StartTime { get; set; }
        static string rhinoSystemDirectory { get; set; }

        static void Main(string[] args)
        {
            Config.Load();
            // Pre-scan for -port so it can be enriched into every log line. Full arg parsing
            // happens below; this is just a peek so the logger has the port from the very first
            // line. Without it, "Logging to ..." and "Registering idle span ..." would land in
            // the console with an empty port column.
            Logging.Init(FindPortArg(args));

            ParseCommandLineArgs(args);

            // Loud warning if the server is starting unauthenticated. The ApiKeyMiddleware
            // only wires up when Config.ApiKey is non-empty, so a missing key means every
            // POST endpoint accepts any caller. Operators sometimes don't realize the env
            // var didn't propagate (running process predates the setx, IIS app pool not
            // recycled, etc.) — this surfaces the problem at startup instead of silently.
            // Skip it when launched by rhino.compute (signalled by -childof:<pid>, which
            // populates Shutdown.ParentProcesses during ParseCommandLineArgs): the parent
            // already prints this same warning, so emitting it per child just duplicates it.
            bool launchedByRhinoCompute = Shutdown.ParentProcesses != null && Shutdown.ParentProcesses.Count > 0;
            if (!launchedByRhinoCompute && string.IsNullOrWhiteSpace(Config.ApiKey))
                Log.Warning("RHINO_COMPUTE_KEY is not set; API authentication is disabled. All endpoints are open to any caller.");

#if DEBUG
            // Uncomment the following to debug with core Rhino source. This
            // tells compute to use a different RhinoCore than what RhinoInside thinks
            // should use.
            // (for McNeel devs only and only those devs who use the same path as Andy)

            //string rhinoSystemDir = @"C:\dev\github\mcneel\rhino9\src4\bin\Debug";
            //if (System.IO.File.Exists(rhinoSystemDir + "\\Rhino.exe"))
            //rhinoSystemDirectory = "/usr/lib/rhino3d";//rhinoSystemDir;
#endif

            if (String.IsNullOrEmpty(rhinoSystemDirectory))
                RhinoInside.Resolver.Initialize();
            else
                RhinoInside.Resolver.Initialize(rhinoSystemDirectory);

            StartTime = DateTime.Now;
            Shutdown.RegisterStartTime(StartTime);
            Log.Information($"Child process started at " + StartTime.ToLocalTime().ToString());

            LogVersions();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var b = webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Cap request body to Config.MaxRequestSize (default 50 MB, override via
                        // RHINO_COMPUTE_MAX_REQUEST_SIZE). Previously this was set to null
                        // (unlimited), which allowed a single caller to send an unbounded body
                        // and exhaust server memory. The env var name matches rhino.compute's
                        // so child processes inherit the parent's configured value automatically.
                        options.Limits.MaxRequestBodySize = Config.MaxRequestSize;
                        if (Config.LocalhostPort > 0)
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

            // When the port wasn't passed via -port:N (standalone launches), Kestrel binds to
            // its default URL once the host starts. Pull the bound port out of IServerAddressesFeature
            // and push it into DynamicPortEnricher so subsequent log lines render as "CG NNNN".
            var lifetime = (Microsoft.Extensions.Hosting.IHostApplicationLifetime)
                host.Services.GetService(typeof(Microsoft.Extensions.Hosting.IHostApplicationLifetime));
            lifetime?.ApplicationStarted.Register(() =>
            {
                var server = (Microsoft.AspNetCore.Hosting.Server.IServer)
                    host.Services.GetService(typeof(Microsoft.AspNetCore.Hosting.Server.IServer));
                var addresses = server?.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
                if (addresses == null) return;
                foreach (var url in addresses.Addresses)
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Port > 0)
                    {
                        DynamicPortEnricher.SetPort(uri.Port);
                        break;
                    }
                }
            });

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
                SplitArg(args[i], out string key, out string value);

                switch (key)
                {
                    case "port":
                        {
                            Config.LocalhostPort = int.Parse(value);
                            //Log.Information($"Parsed port = {Config.LocalhostPort}");
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
                            Log.Debug($"Registering idle span value of {span} seconds");
                            Shutdown.RegisterIdleSpan(span);
                        }
                        break;
                    case "rhinosysdir":
                        rhinoSystemDirectory = value;
                        break;
                    case "load-grasshopper":
                        {
                            // Set environment variable so Config.Load() picks it up
                            Environment.SetEnvironmentVariable("RHINO_COMPUTE_LOAD_GRASSHOPPER", value);
                            Log.Information($"Grasshopper loading set to: {value}");
                        }
                        break;
                    case "apikey":
                        {
                            Environment.SetEnvironmentVariable("RHINO_COMPUTE_KEY", value);
                            Log.Information("API key set from command line");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        static int FindPortArg(string[] args)
        {
            foreach (var arg in args)
            {
                SplitArg(arg, out string key, out string value);
                if (key == "port" && int.TryParse(value, out int port))
                    return port;
            }
            return 0;
        }

        static void SplitArg(string arg, out string key, out string value)
        {
            key = arg;
            value = string.Empty;

            int i = arg.IndexOf(":");
            if (i > 0)
            {
                key = arg.Substring(0, i);
                value = arg.Substring(i + 1, arg.Length - key.Length - 1);
                
                // cleanup key, value
                key = key.ToLowerInvariant().TrimStart('-');
            }
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
            Log.Debug("Rhino system directory: {Path}", RhinoInside.Resolver.RhinoSystemDirectory);
        }
    }

    public static class RhinoGetModule
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/sdk", context => SdkEndpoint(context, app));
            app.MapGet("/sdk/csharp", context => CSharpSdk(context));

            foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            {
                app.MapGet(endpoint.PathURL, endpoint.Get);
            }
        }

        static async Task CSharpSdk(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string fileContents;
            using (Stream resourceStream = typeof(FixedEndPointsModule).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
            {
                if (resourceStream != null)
                {
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        fileContents = await reader.ReadToEndAsync();
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }
            var result = new StringBuilder();
            result.AppendLine(fileContents);
            await context.Response.WriteAsync(result.ToString());
        }

        static async Task SdkEndpoint(HttpContext context, IEndpointRouteBuilder app)
        {
            var result = new StringBuilder("<!DOCTYPE html><html><body>");
            result.AppendLine($" <a href=\"/sdk/csharp\">C# SDK</a><BR>");
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
            result.AppendLine("</p></body></html>");
            await context.Response.WriteAsync(result.ToString());
        }
    }

    public static class RhinoPostModule
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            foreach (var endpoint in GeometryEndPoint.AllEndPoints)
            {
                app.MapPost(endpoint.PathURL, endpoint.Post);
            }
        }
    }
}
