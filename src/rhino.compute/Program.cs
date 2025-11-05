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
        }

        static System.Diagnostics.Process _parentProcess;
        static System.Timers.Timer _selfDestructTimer;

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

                // Set runtime options
                ComputeChildren.SpawnCount = o.ChildCount;
                ComputeChildren.SpawnOnStartup = o.SpawnOnStartup;
                ComputeChildren.ChildIdleSpan = new System.TimeSpan(0, 0, o.IdleSpanSeconds);
                int parentProcessId = o.ChildOf;
                if (parentProcessId > 0)
                    _parentProcess = System.Diagnostics.Process.GetProcessById(parentProcessId);
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
                .Filter.ByExcluding("RequestPath in ['/healthcheck', '/favicon.ico']")
                .WriteTo.Console(outputTemplate: "RC  [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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

            if(_parentProcess?.MainModule != null)
            {
                var parentPath = _parentProcess.MainModule.FileName;
                if (Path.GetFileName(parentPath) == "Rhino.exe")
                {
                    ComputeChildren.RhinoSysDir = Directory.GetParent(parentPath).FullName;
                }
            }

            Log.Information($"Rhino compute started at {DateTime.Now.ToLocalTime()}");
            Log.Debug($"Config:");
            Log.Debug("  Max Request Size = {RequestSize}", (Config.MaxRequestSize / 1024.0 / 1024.0).ToString("F2") + " MB");
            Log.Debug("  Timeout = {Timeout}", FormatTimeout(Config.ReverseProxyRequestTimeout));
            Log.Debug("  Child Count = {ChildCount}", ComputeChildren.SpawnCount.ToString());
            Log.Debug("  Spawn Children At Startup = {SpawnChild}", ComputeChildren.SpawnOnStartup.ToString());
            bool loadGrasshopper = true;
            loadGrasshopper = Boolean.TryParse(Environment.GetEnvironmentVariable("RHINO_COMPUTE_LOAD_GRASSHOPPER"), out var loadGH) ? loadGH : true;
            Log.Debug("  Load Grasshopper = {LoadGH}", loadGrasshopper.ToString());
            bool createHeadlessDoc = false;
            createHeadlessDoc = Boolean.TryParse(Environment.GetEnvironmentVariable("RHINO_COMPUTE_CREATE_HEADLESS_DOC"), out var createHeadless) ? createHeadless : false;
            Log.Debug("  Create Headless Document = {CreateHeadlessDoc}", createHeadlessDoc.ToString());
            Log.Debug("  Log Path = {LogPath}", Config.LogPath);

            var logger = host.Services.GetRequiredService<ILogger<ReverseProxyModule>>();
            ReverseProxyModule.InitializeConcurrentRequestLogging(logger);

            if (_parentProcess != null)
            {
                _selfDestructTimer = new System.Timers.Timer(1000);
                _selfDestructTimer.Elapsed += (s, e) =>
                {
                    if (_parentProcess.HasExited)
                    {
                        _selfDestructTimer.Stop();
                        _parentProcess = null;
                        Console.WriteLine("self-destruct");
                        Log.Information($"Self-destruct called at {DateTime.Now.ToLocalTime()}");
                        host.StopAsync();
                    }
                };
                _selfDestructTimer.AutoReset = true;
                _selfDestructTimer.Start();
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
            if (_parentProcess != null && _parentProcess.ProcessName.Contains("rhino", StringComparison.OrdinalIgnoreCase))
            {
                return (_parentProcess.Id == processId);
            }
            return false;
        }
    }
}
