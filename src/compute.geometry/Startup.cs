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
        // https://github.com/mcneel/rhino/blob/e1192835cbf03f662d0cf857ee9239b84109eeed/src4/rhino4/Plug-ins/RhinoCodePlugins/RhinoCodePlugin/AssemblyInfo.cs
        static readonly Guid s_rhinoCodePluginId = new Guid("c9cba87a-23ce-4f15-a918-97645c05cde7");

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
            Log.Information("(1/3) Loading grasshopper");
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
            if (runheadless != null)
                runheadless.Invoke(pluginObject, null);

            Rhino.RhinoApp.SendWriteToConsole = false;

            Log.Information("(2/3) Loading compute plug-ins");
            var loadComputePlugins = typeof(Rhino.PlugIns.PlugIn).GetMethod("LoadComputeExtensionPlugins");
            if (loadComputePlugins != null)
                loadComputePlugins.Invoke(null, null);

            // NOTE:
            // Ensure RhinoCode plugin (Rhino plugin) is loaded. This plugin registers scripting
            // languages and starts the scripting server that communicates with rhinocode CLI. It also makes
            // the ScriptEditor and RhinoCodeLogs commands available.
            // For Rhino.Compute use cases, the ScriptEditor and rhinocode CLI are not going to be used.
            // The first time a Grasshopper definition with any scripting component on it is passed to Compute,
            // the script environments (especially python 3) will be initialized. This increases the execution
            // time on the first run on any script component. However after that the script components should run
            // normally. The scripting environment will only re-initialize when a new version of Rhino is installed.
            Log.Information("(3/3) Loading rhino scripting plugin");
            if (!Rhino.PlugIns.PlugIn.LoadPlugIn(s_rhinoCodePluginId))
            {
                // If plugin load fails, let compute run, but log the error
                Log.Error("Error loading rhino scripting plugin. Grasshopper script components are going to fail");
            }

            //ApiKey.Initialize(pipelines);
            //Rhino.Runtime.HostUtils.RegisterComputeEndpoint("grasshopper", typeof(Endpoints.GrasshopperEndpoint));
        }

    }
}
