namespace rhino.compute
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using CommandLine;

    public class Program
    {
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
             HelpText = "Seconds that child compute.geometry processes should remain open between requests")]
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
                _parentProcess = System.Diagnostics.Process.GetProcessById(o.ChildOf);
                port = o.Port;
            });
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var b = webBuilder.UseStartup<Startup>();
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
    }
}
