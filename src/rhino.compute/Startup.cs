namespace rhino.compute
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System.Text.Json;

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
        }

        public void Configure(IApplicationBuilder app)
        {
            // Global exception handler. Sits at the very top of the pipeline so it catches
            // anything thrown by downstream middleware or endpoint handlers. Logs the
            // exception via Serilog, then returns a structured JSON 500 response. Stack
            // traces are only included when Config.Debug is true so production builds
            // don't leak implementation details to callers.
            app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
            {
                var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;

                string category = ex == null ? null
                    : ex.GetType().Name.Contains("Json") ? "Malformed JSON received"
                    : ex is FormatException ? "Invalid format"
                    : ex is ArgumentException ? "Invalid argument"
                    : ex.GetType().Name;
                string message = ex == null
                    ? "Unknown error"
                    : (category != null ? $"{category}: {ex.Message}" : ex.Message);

                Log.Error(ex, "Unhandled exception during {Method} {Path}: {Category}",
                    ctx.Request.Method, ctx.Request.Path, category ?? "Unknown");

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";

                string[] stack = ex?.StackTrace?
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.TrimStart())
                    .ToArray() ?? Array.Empty<string>();

                object body = Config.Debug
                    ? new { error = "Internal Server Error", message, stackTrace = stack }
                    : new { error = "Internal Server Error", message = "An unexpected error occurred. Check server logs for details." };
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(body));
            }));

            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCors();
            if (!String.IsNullOrEmpty(Config.ApiKey))
                app.UseMiddleware<ApiKeyMiddleware>();
            app.UseEndpoints(builder =>
            {
                builder.MapHealthChecks("/healthcheck");
                ReverseProxyModule.MapEndpoints(builder);
            });
        }
    }
}
