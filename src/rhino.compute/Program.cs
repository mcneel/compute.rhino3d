namespace rhino.compute
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using CommandLine;
    using NLog.Extensions.Logging;

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
        public static void Main(string[] args)
        {
            int port = -1;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                ComputeChildren.SpawnCount = o.ChildCount;
                ComputeChildren.ChildIdleSpan = new System.TimeSpan(0, 0, o.IdleSpanSeconds);
                int parentProcessId = o.ChildOf;
                if (parentProcessId > 0)
                    _parentProcess = System.Diagnostics.Process.GetProcessById(parentProcessId);
                port = o.Port;
            });

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var b = webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Handle requests up to 50 MB
                        options.Limits.MaxRequestBodySize = 52428800;
                    })
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true)
                    .ConfigureLogging((hostingContext, logging) => {
                        logging.AddNLog(hostingContext.Configuration.GetSection("Logging"));
                    });

                    if (port > 0)
                    {
                        b.UseUrls($"http://localhost:{port}");
                        ComputeChildren.ParentPort = port;
                    }

                }).Build();

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
