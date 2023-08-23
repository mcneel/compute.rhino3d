using System;
using System.IO;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace compute.geometry
{
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
            RhinoCoreStartup();
            
            app.UseRouting();
            app.UseCors();
            //if (!String.IsNullOrEmpty(Config.ApiKey))
            //    app.UseMiddleware<ApiKeyMiddleware>();
            app.UseEndpoints(builder =>
            {
                builder.MapHealthChecks("/healthcheck");
                builder.MapCarter();
            });
        }

        void RhinoCoreStartup()
        {
            Program.RhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
            Environment.SetEnvironmentVariable("RHINO_TOKEN", null, EnvironmentVariableTarget.Process);
            Rhino.Runtime.HostUtils.OnExceptionReport += (source, ex) => {
                Log.Error(ex, "An exception occurred while processing request");
                Logging.LogExceptionData(ex);
            };

            Rhino.RhinoApp.SendWriteToConsole = true;

            // Load GH at startup so it can get initialized on the main thread
            Log.Information("(1/2) Loading grasshopper");
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
            if (runheadless != null)
                runheadless.Invoke(pluginObject, null);

            Rhino.RhinoApp.SendWriteToConsole = false;

            Log.Information("(2/2) Loading compute plug-ins");
            var loadComputePlugins = typeof(Rhino.PlugIns.PlugIn).GetMethod("LoadComputeExtensionPlugins");
            if (loadComputePlugins != null)
                loadComputePlugins.Invoke(null, null);

            //ApiKey.Initialize(pipelines);
            //Rhino.Runtime.HostUtils.RegisterComputeEndpoint("grasshopper", typeof(Endpoints.GrasshopperEndpoint));
        }

    }
}
