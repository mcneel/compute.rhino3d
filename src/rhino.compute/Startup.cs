namespace rhino.compute
{
    using Carter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using rhino.compute.Logging;
    using Serilog;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCarter();
            services.AddSingleton<ILog, LogNLog>();
        }

        public void Configure(IApplicationBuilder app, ILog logger)
        {
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapCarter());
            app.ConfigureExceptionHandler(logger);
        }
    }
}
