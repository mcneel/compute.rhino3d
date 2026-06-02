using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace compute.geometry
{
    public class Startup
    {

        //https://github.com/mcneel/rhino/blob/e1192835cbf03f662d0cf857ee9239b84109eeed/src4/rhino4/Plug-ins/RhinoCodePlugins/RhinoCodePlugin/AssemblyInfo.cs
        static readonly Guid s_rhinoCodePluginId = new Guid("c9cba87a-23ce-4f15-a918-97645c05cde7");

        //https://github.com/mcneel/rhino/blob/8.x/src4/rhino4/Plug-ins/Commands/Properties/AssemblyInfo.cs
        static readonly Guid s_rhinoCommandsPluginId = new Guid("02bf604d-799c-4cc2-830e-8d72f21b14b7");

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
            RhinoCoreStartup();

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
                await ctx.Response.WriteAsync(JsonConvert.SerializeObject(body));
            }));

            app.UseRouting();
            app.UseCors();
            if (!string.IsNullOrEmpty(Config.ApiKey))
                app.UseMiddleware<ApiKeyMiddleware>();
            app.UseEndpoints(builder =>
            {
                builder.MapHealthChecks("/healthcheck");
                FixedEndPointsModule.MapEndpoints(builder);
                ResthopperEndpointsModule.MapEndpoints(builder);
                RhinoGetModule.MapEndpoints(builder);
                RhinoPostModule.MapEndpoints(builder);
            });
        }

        void RhinoCoreStartup()
        {
            Program.RhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            if (Config.Debug)
                Rhino.RhinoApp.SendWriteToConsole = true;
            
            Environment.SetEnvironmentVariable("RHINO_TOKEN", null, EnvironmentVariableTarget.Process);
            Rhino.Runtime.HostUtils.OnExceptionReport += (source, ex) =>
            {
                Log.Error(ex, "An exception occurred while processing request");
                Logging.LogExceptionData(ex);
            };

            // NOTE:
            // andyopayne 11/19/2024 (RH-84777)
            // The commands.rhp needs to be loaded so that some features suchs as the gltf exporter will work.
            // This is a temporary solution until the gltf exporter is moved into Rhinocommon or Rhino.UI
            Log.Information("(1/4) Loading rhino commands plugin");
            if (Rhino.PlugIns.PlugIn.LoadPlugIn(s_rhinoCommandsPluginId))
            {
                Log.Information("Successfully loaded commands plugin");
            }
            else
            {
                Log.Error("Error loading rhino commands plugin.");
            }

            // NOTE:
            // eirannejad 10/02/2024 (COMPUTE-268)
            // Ensure RhinoCode plugin (Rhino plugin) is loaded. This plugin registers scripting
            // languages and starts the scripting server that communicates with rhinocode CLI. It also makes
            // the ScriptEditor and RhinoCodeLogs commands available.
            // For Rhino.Compute use cases, the ScriptEditor and rhinocode CLI are not going to be used.
            // The first time a Grasshopper definition with any scripting component on it is passed to Compute,
            // the script environments (especially python 3) will be initialized. This increases the execution
            // time on the first run on any script component. However after that the script components should run
            // normally. The scripting environment will only re-initialize when a new version of Rhino is installed.
            // eirannejad 12/3/2024 (COMPUTE-268)
            // This load is placed before Grasshopper in case GH needs to load any plugins published by the
            // new scripting tools in Rhino >= 8
            Log.Information("(2/4) Loading rhino scripting plugin");
#if LINUX
            // On Linux, we need to load the plugin manually from the expected path since the .rhp extension is not registered
            var rhinoCodePluginPath = RhinoInside.Resolver.RhinoSystemDirectory + "/Plug-ins/RhinoCode/RhinoCodePlugin.rhp";
            var rc_pluginresult = Rhino.PlugIns.PlugIn.LoadPlugIn(rhinoCodePluginPath, out Guid rhinoCodePluginId);
            if( rc_pluginresult == Rhino.PlugIns.LoadPlugInResult.Success )
            {
#else
            if (Rhino.PlugIns.PlugIn.LoadPlugIn(s_rhinoCodePluginId))
            {
#endif
                Log.Information("Successfully loaded scripting plugin");

                // eirannejad 12/3/2024 (COMPUTE-268)
                // now configuring scripting env to avoid using rhino progressbar and
                // dump init and package install messages to Rhino.RhinoApp.Write
                if (Rhino.RhinoApp.GetPlugInObject(s_rhinoCodePluginId) is object rhinoCodeController)
                {
                    ((dynamic)rhinoCodeController).SendReportsToConsole = true;
                    Log.Information("Configured scripting plugin for compute");
                }
            }
            // If plugin load fails, let compute run, but log the error
            else
            {
                Log.Error("Error loading rhino scripting plugin. Grasshopper script components are going to fail");
            }

            // Load GH at startup so it can get initialized on the main thread
            if (Config.LoadGrasshopper)
            {
                Log.Information("(3/4) Loading grasshopper");

#if LINUX
                var ghpath = RhinoInside.Resolver.RhinoSystemDirectory + "/Plug-ins/Grasshopper/GrasshopperPlugin.rhp";
                var pluginresult = Rhino.PlugIns.PlugIn.LoadPlugIn(ghpath, out Guid ghid);
                var pluginObject = Rhino.RhinoApp.GetPlugInObject(ghid) as Grasshopper.Plugin.GH_RhinoScriptInterface;
                if(pluginObject != null)
                    pluginObject.RunHeadless();
#else
                var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
                var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
                if (runheadless != null)
                    runheadless.Invoke(pluginObject, null);
#endif

                // Only emit the "Loaded assembly: X" burst when we're running as a child of
                // rhino.compute (signalled by the -childof:<pid> arg, which populates
                // Shutdown.ParentProcesses). In that mode CreateNoWindow=true on the child
                // ProcessStartInfo suppresses Grasshopper's native "* Loading X assembly..."
                // chatter, so re-emitting via Serilog restores per-plugin visibility through
                // the CG-prefixed channel. When launched standalone, the native chatter is
                // already visible on the console and adding our burst would just duplicate it,
                // so we skip the enumeration.
                bool launchedByRhinoCompute = Shutdown.ParentProcesses != null && Shutdown.ParentProcesses.Count > 0;
                if (launchedByRhinoCompute)
                {
                    try
                    {
                        foreach (var lib in Grasshopper.Instances.ComponentServer.Libraries)
                        {
                            if (lib != null && !string.IsNullOrEmpty(lib.Name))
                                Log.Information("Loaded assembly: {Name}", lib.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Unable to enumerate Grasshopper libraries after RunHeadless");
                    }
                }
            }
            else
            {
                Log.Information("(3/4) Skipping grasshopper (disabled via RHINO_COMPUTE_LOAD_GRASSHOPPER)");
            }

            Log.Information("(4/4) Loading compute plug-ins");
            var loadComputePlugins = typeof(Rhino.PlugIns.PlugIn).GetMethod("LoadComputeExtensionPlugins");
            if (loadComputePlugins != null)
                loadComputePlugins.Invoke(null, null);

        }

    }
}
