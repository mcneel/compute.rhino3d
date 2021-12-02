namespace rhino.compute
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Carter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using rhino.compute.Logging;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCarter();
            services.AddSingleton<ILog, LogNLog>();
        }

        public void Configure(IApplicationBuilder app, ILog logger)
        {
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapCarter());
            app.ConfigureExceptionHandler(logger);
        }
    }
}
