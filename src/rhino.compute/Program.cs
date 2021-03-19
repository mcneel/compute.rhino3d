namespace rhino.compute
{
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
             HelpText = @"Process name of parent process. Compute watches for the existence 
of any process by this name and if there are none; it will shut down")]
            public string ChildOf { get; set; }

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

        static string _parentProcessName;
        static System.Timers.Timer _selfDestructTimer;

        public static void Main(string[] args)
        {
            int port = -1;
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                ComputeChildren.SpawnCount = o.ChildCount;
                ComputeChildren.ChildIdleSpan = new System.TimeSpan(0, 0, o.IdleSpanSeconds);
                _parentProcessName = o.ChildOf;
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

            if (!string.IsNullOrWhiteSpace(_parentProcessName))
            {
                _selfDestructTimer = new System.Timers.Timer(1000);
                _selfDestructTimer.Elapsed += (s, e) =>
                {
                    var procs = System.Diagnostics.Process.GetProcessesByName(_parentProcessName);
                    if (procs==null || procs.Length<1)
                    {
                        host.StopAsync();
                    }
                };
                _selfDestructTimer.AutoReset = true;
            }

            host.Run();
        }
    }
}
