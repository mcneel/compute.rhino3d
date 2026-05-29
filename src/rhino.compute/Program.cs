namespace rhino.compute
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using CommandLine;
    using Serilog;
    using Serilog.Events;
    using System.IO;
    using System.Globalization;
    using System.Threading;
    using Serilog.Templates;
    using System.Collections.Generic;

    public class Program
    {
        /// <summary>
        /// Command line options for rhino.compute.exe. An example of the syntax is
        /// rhino.compute.exe --childcount 8
        /// This would launch rhino.compute with 8 child compute.geometry.exe processes
        /// </summary>
        class Options
        {
            [Option("childof",
             Required = false,
             HelpText = @"Process Handle of parent process. Compute watches for the existence 
                of this handle and will shut down when this process has exited")]
            public int ChildOf { get; set; }

            [Option("childcount",
             Required = false,
             HelpText = "Number of child compute.geometry processes to manage")]
            public int ChildCount { get; set; } = 4;

            [Option("spawn-on-startup",
             Required = false,
             Default = false,
             HelpText = "Determines whether to launch a child compute.geometry process when rhino.compute gets started")]
            public bool SpawnOnStartup { get; set; }

            [Option("load-children-sequentially",
             Required = false,
             Default = false,
             HelpText = "When set, child compute.geometry processes spawn one at a time (each child finishes loading before the next starts). Default is parallel spawning for faster warmup. Use this on memory-constrained VMs where N parallel Rhino+Grasshopper loads can exhaust RAM.")]
            public bool LoadChildrenSequentially { get; set; }

            [Option("idlespan", 
             Required = false,
             HelpText = 
                @"Seconds that child compute.geometry processes should remain open between requests. (Default 1 hour)
                When rhino.compute.exe does not receive requests to solve over a period of 'idlespan' seconds, child
                compute.geometry.exe processes will shut down and stop incurring core hour billing. At some date in the
                future when a new request is received, the child processes will be relaunched which will cause a delay on
                requests while the child processes are launching.")]
            public int IdleSpanSeconds { get; set; } = 60 * 60;

            [Option("port",
              Required = false,
              HelpText = "Port number to run rhino.compute on")]
            public int Port { get; set; } = -1;

            [Option("max-request-size",
              Required = false,
              HelpText = "Maximum request body size in bytes (default: 52428800 = 50MB)")]
            public long MaxRequestSize { get; set; } = -1;

            [Option("apikey",
              Required = false,
              HelpText = "API key for authentication (leave empty to disable)")]
            public string ApiKey { get; set; }

            [Option("timeout",
              Required = false,
              HelpText = "Request timeout in seconds (default: 100)")]
            public int TimeoutSeconds { get; set; } = -1;

            [Option("load-grasshopper",
              Required = false,
              HelpText = "Load Grasshopper plugin in child processes (default: true)")]
            public bool? LoadGrasshopper { get; set; }

            [Option("create-headless-doc",
              Required = false,
              HelpText = "Create a new headless Rhino doc upon each received request (default: false)")]
            public bool? CreateHeadlessDoc { get; set; }

            [Option("block-private-urls",
              Required = false,
              Default = false,
              HelpText = "When set, compute.geometry refuses to fetch server-side URLs whose hostname resolves to a private, loopback, or link-local IP address. Recommended for public-facing deployments to defend against SSRF attacks targeting cloud metadata endpoints (e.g. 169.254.169.254 on AWS/Azure/GCP) and internal LAN services. Default is off so deployments fetching from internal hosts continue to work.")]
            public bool BlockPrivateUrls { get; set; }
        }

        static System.Diagnostics.Process parentProcess;
        static System.Timers.Timer selfDestructTimer;

        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Parse command line arguments BEFORE Config.Load() so we can set environment variables
            int port = -1;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                // Set environment variables from command line args (child processes will inherit)
                if (o.MaxRequestSize > 0)
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_MAX_REQUEST_SIZE", o.MaxRequestSize.ToString());

                if (!string.IsNullOrEmpty(o.ApiKey))
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_KEY", o.ApiKey);

                if (o.TimeoutSeconds > 0)
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_TIMEOUT", o.TimeoutSeconds.ToString());

                if (o.LoadGrasshopper.HasValue)
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_LOAD_GRASSHOPPER", o.LoadGrasshopper.Value ? "true" : "false");

                if (o.CreateHeadlessDoc.HasValue)
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_CREATE_HEADLESS_DOC", o.CreateHeadlessDoc.Value ? "true" : "false");

                // --block-private-urls enables SSRF protection in spawned compute.geometry
                // children by setting the env var they read at startup. Only set when the
                // flag is present so an external RHINO_COMPUTE_BLOCK_PRIVATE_URLS=true (set
                // before launching rhino.compute) still takes effect without the flag.
                if (o.BlockPrivateUrls)
                    Environment.SetEnvironmentVariable("RHINO_COMPUTE_BLOCK_PRIVATE_URLS", "true");

                // Set runtime options. ChildCount is capped at ComputeChildren.MaxChildren
                // (same cap that protects the /launch?children=N endpoint) so the Config
                // block below prints the actual-effective value and downstream code never
                // sees an unsafe value.
                int requestedChildren = o.ChildCount;
                if (requestedChildren > ComputeChildren.MaxChildren)
                {
                    Log.Warning("--childcount capped from {Requested} to {Cap}", requestedChildren, ComputeChildren.MaxChildren);
                    requestedChildren = ComputeChildren.MaxChildren;
                }
                ComputeChildren.SpawnCount = requestedChildren;
                ComputeChildren.SpawnOnStartup = o.SpawnOnStartup;
                ComputeChildren.LoadChildrenSequentially = o.LoadChildrenSequentially;
                ComputeChildren.ChildIdleSpan = new System.TimeSpan(0, 0, o.IdleSpanSeconds);
                int parentProcessId = o.ChildOf;
                if (parentProcessId > 0)
                    parentProcess = System.Diagnostics.Process.GetProcessById(parentProcessId);
                port = o.Port;
            });


            // Now load config (will use environment variables set above)
            Config.Load();

            var path = System.IO.Path.Combine(Config.LogPath, "log-rhino-compute-.txt");
            var limit = Config.LogRetainDays;
            var level = Config.Debug ? LogEventLevel.Debug : LogEventLevel.Information;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                // Silence ASP.NET Core's built-in "An unhandled exception has occurred while
                // executing the request." log emitted by ExceptionHandlerMiddleware. Our own
                // app.UseExceptionHandler handler logs a categorized, formatted version with
                // the same stack trace, so without this override the same exception shows up
                // twice on the console.
                .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Fatal)
                .Filter.ByExcluding("RequestPath in ['/healthcheck', '/favicon.ico']")
                .WriteTo.Console(
                    outputTemplate: "RC  [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate)
                .WriteTo.File(new ExpressionTemplate("RC   [{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}"), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);
            Log.Logger = loggerConfig.CreateLogger();

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var b = webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Handle requests up to 50 MB
                        options.Limits.MaxRequestBodySize = Config.MaxRequestSize;
                    })
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true);

                    if (port > 0)
                    {
                        b.UseUrls($"http://localhost:{port}");
                        ComputeChildren.ParentPort = port;
                    }

                }).Build();

            if(parentProcess?.MainModule != null)
            {
                var parentPath = parentProcess.MainModule.FileName;
                if (Path.GetFileName(parentPath) == "Rhino.exe")
                {
                    ComputeChildren.RhinoSysDir = Directory.GetParent(parentPath).FullName;
                }
            }

            Log.Information($"Rhino compute started at {DateTime.Now.ToLocalTime()}");

            // Loud warning if the proxy is starting unauthenticated. The ApiKeyMiddleware
            // only wires up when Config.ApiKey is non-empty, so a missing key means every
            // endpoint accepts any caller. Operators sometimes don't realize the env var
            // didn't propagate (running process predates the setx, IIS app pool not
            // recycled, etc.) — this surfaces the problem at startup instead of silently.
            if (string.IsNullOrWhiteSpace(Config.ApiKey))
                Log.Warning("RHINO_COMPUTE_KEY is not set; API authentication is disabled. All endpoints are open to any caller.");

            Log.Debug($"Config:");
            Log.Debug("  Max Request Size = {RequestSize}", (Config.MaxRequestSize / 1024.0 / 1024.0).ToString("F2") + " MB");
            Log.Debug("  Timeout = {Timeout}", FormatTimeout(Config.ReverseProxyRequestTimeout));
            Log.Debug("  Child Count = {ChildCount}", ComputeChildren.SpawnCount.ToString());
            Log.Debug("  Spawn Children At Startup = {SpawnChild}", ComputeChildren.SpawnOnStartup.ToString());
            Log.Debug("  Load Children Sequentially = {LoadSequentially}", ComputeChildren.LoadChildrenSequentially.ToString());
            bool loadGrasshopper = true;
            loadGrasshopper = Boolean.TryParse(Environment.GetEnvironmentVariable("RHINO_COMPUTE_LOAD_GRASSHOPPER"), out var loadGH) ? loadGH : true;
            Log.Debug("  Load Grasshopper = {LoadGH}", loadGrasshopper.ToString());
            bool createHeadlessDoc = false;
            createHeadlessDoc = Boolean.TryParse(Environment.GetEnvironmentVariable("RHINO_COMPUTE_CREATE_HEADLESS_DOC"), out var createHeadless) ? createHeadless : false;
            Log.Debug("  Create Headless Document = {CreateHeadlessDoc}", createHeadlessDoc.ToString());
            bool blockPrivateUrls = Boolean.TryParse(Environment.GetEnvironmentVariable("RHINO_COMPUTE_BLOCK_PRIVATE_URLS"), out var blockPrivate) && blockPrivate;
            Log.Debug("  Block Private URLs = {BlockPrivateUrls}", blockPrivateUrls.ToString());
            Log.Debug("  Log Path = {LogPath}", Config.LogPath);

            var logger = host.Services.GetRequiredService<ILogger<ReverseProxyModule>>();
            ReverseProxyModule.InitializeConcurrentRequestLogging(logger);

            // On clean shutdown of rhino.compute (Ctrl-C, IIS app pool recycle, host.StopAsync()
            // from selfDestructTimer when parent exits), gracefully stop spawned compute.geometry
            // children. Hard-crash scenarios (kill -9, segfault) bypass this hook — children fall
            // back to the existing 5-second HasExited poll in their own Shutdown.cs TimerTask.
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                Log.Information("rhino.compute shutting down; signaling compute.geometry children");
                try { ComputeChildren.ShutdownChildren(); }
                catch (Exception ex) { Log.Warning("Error during child shutdown: {Message}", ex.Message); }
            });

            if (parentProcess != null)
            {
                selfDestructTimer = new System.Timers.Timer(1000);
                selfDestructTimer.Elapsed += (s, e) =>
                {
                    if (parentProcess.HasExited)
                    {
                        selfDestructTimer.Stop();
                        parentProcess = null;
                        Console.WriteLine("self-destruct");
                        Log.Information($"Self-destruct called at {DateTime.Now.ToLocalTime()}");
                        host.StopAsync();
                    }
                };
                selfDestructTimer.AutoReset = true;
                selfDestructTimer.Start();
            }
            host.Run();
        }
        private static string FormatTimeout(int totalSeconds)
        {
            var ts = TimeSpan.FromSeconds(totalSeconds);
            var parts = new List<string>();
            if (ts.Days > 0)
                parts.Add($"{ts.Days} day{(ts.Days == 1 ? "" : "s")}");
            if (ts.Hours > 0 || ts.Days > 0)
                parts.Add($"{ts.Hours} hr{(ts.Hours == 1 ? "" : "s")}");
            if (ts.Minutes > 0 || ts.Hours > 0 || ts.Days > 0)
                parts.Add($"{ts.Minutes} min{(ts.Minutes == 1 ? "" : "s")}");
            parts.Add($"{ts.Seconds} sec{(ts.Seconds == 1 ? "" : "s")}");
            return string.Join(" ", parts);
        }

        public static bool IsParentRhinoProcess(int processId)
        {
            if (parentProcess != null && parentProcess.ProcessName.Contains("rhino", StringComparison.OrdinalIgnoreCase))
            {
                return (parentProcess.Id == processId);
            }
            return false;
        }
    }
}
