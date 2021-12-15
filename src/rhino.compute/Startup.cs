namespace rhino.compute
{
    using Carter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
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
