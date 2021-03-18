namespace rhino.compute
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .Build();
            
            var logger = host.Services.GetRequiredService<ILogger<ReverseProxyModule>>();
            ReverseProxyModule.InitializeConcurrentRequestLogging(logger);
            
            host.Run();
        }
    }
}
