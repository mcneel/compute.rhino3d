namespace rhino.compute
{
    using System;
    using Carter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader();
                    });
            });
            services.AddHealthChecks();
            services.AddCarter();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCors();
            if (!String.IsNullOrEmpty(Config.ApiKey))
                app.UseMiddleware<ApiKeyMiddleware>();
            app.UseEndpoints(builder =>
            {
                builder.MapHealthChecks("/healthcheck");
                builder.MapCarter();
            });
        }
    }
}
