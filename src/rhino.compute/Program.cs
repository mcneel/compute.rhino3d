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
    using Microsoft.Extensions.Configuration;
    using System.IO;

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
        }

        static System.Diagnostics.Process _parentProcess;
        static System.Timers.Timer _selfDestructTimer;
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Config.Load();
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .Filter.ByExcluding("RequestPath in ['/healthcheck', '/favicon.ico']")
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

            var arguments = Environment.GetEnvironmentVariable("RHINO_COMPUTE_ARGUMENTS");
            if (!string.IsNullOrEmpty(arguments))
            {
                args = arguments.Split(" ");
            }
            
            var rhinoPath = Environment.GetEnvironmentVariable("RHINO_DYLD_LIBRARY_PATH");
            if (rhinoPath != null)
            {
                Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", rhinoPath);
            }

            int port = -1;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                ComputeChildren.SpawnCount = o.ChildCount;
                ComputeChildren.SpawnOnStartup = o.SpawnOnStartup;
                ComputeChildren.ChildIdleSpan = new System.TimeSpan(0, 0, o.IdleSpanSeconds);
                int parentProcessId = o.ChildOf;
                if (parentProcessId > 0)
                    _parentProcess = System.Diagnostics.Process.GetProcessById(parentProcessId);
                port = o.Port;
            });

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
