using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.PlugIns;

namespace compute.geometry
{
    public static class FixedEndPointsModule
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("", HomePage);
            app.MapGet("version", GetVersion);
            app.MapGet("servertime", ServerTime);
            app.MapGet("plugins/rhino/installed", GetInstalledPluginsRhino);
            app.MapGet("plugins/gh/installed", GetInstalledPluginsGrasshopper);
            app.MapPost("cache/purge", PurgeCache);
            app.MapPost("shutdown", ShutdownChild);
        }

        // POST /shutdown — graceful self-shutdown for this compute.geometry child. Auth-gated
        // by ApiKeyMiddleware (POST). Used by rhino.compute when its ApplicationStopping
        // lifecycle hook fires (clean parent exit), and by the /shutdown-children +
        // /recycle-children endpoints in rhino.compute for manual control. The existing
        // 5-second self-monitoring TimerTask in Shutdown.cs remains as the fallback for
        // hard-crash scenarios where the parent dies without running its lifecycle hooks.
        //
        // Responds 202 Accepted immediately and triggers app.StopAsync() on a background
        // task so the response can flush before the host stops.
        static Task ShutdownChild(HttpContext ctx)
        {
            Serilog.Log.Information("Received /shutdown request from {RemoteIp}", ctx.Connection.RemoteIpAddress);
            var lifetime = ctx.RequestServices.GetService<IHostApplicationLifetime>();
            ctx.Response.StatusCode = 202;
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);  // let the response flush before stopping the host
                lifetime?.StopApplication();
            });
            return Task.CompletedTask;
        }

        static void HomePage(HttpContext context)
        {
            context.Response.Redirect("https://www.rhino3d.com/compute");
        }

        static async Task GetVersion(HttpContext ctx)
        {
            var values = new Dictionary<string, string>
            {
                { "rhino", Rhino.RhinoApp.Version.ToString() },
                { "compute", Assembly.GetExecutingAssembly().GetName().Version.ToString() }
            };
            string git_sha = null; // appveyor will replace this
            values.Add("git_sha", git_sha);

            ctx.Response.ContentType= "application/json";
            await ctx.Response.WriteAsJsonAsync(values);
        }

        static async Task ServerTime(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(DateTime.UtcNow);
        }

        static async Task GetInstalledPluginsRhino(HttpContext ctx)
        {
            var rhPluginInfo = new SortedDictionary<string, string>();
            foreach (var k in Rhino.PlugIns.PlugIn.GetInstalledPlugIns().Keys)
            {
                var info = Rhino.PlugIns.PlugIn.GetPlugInInfo(k);
                //Could also use: info.IsLoaded
                if (info != null && !rhPluginInfo.ContainsKey(info.Name) && !info.ShipsWithRhino)
                {
                    rhPluginInfo.Add(info.Name, info.Version);
                }
            }

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(rhPluginInfo);
        }

        // POST /cache/purge — wipes the solve-results / URL-data cache. Does NOT touch
        // the definition cache, since active clients may hold Pointer references to
        // entries there and a purge would cause subsequent /grasshopper calls with those
        // pointers to fail. Operators expecting memory relief without breaking pointer-
        // based flows should use this endpoint. Auth-gated by ApiKeyMiddleware (POST
        // method requires the RhinoComputeKey header when Config.ApiKey is set).
        static async Task PurgeCache(HttpContext ctx)
        {
            long removed = DataCache.PurgeSolveResults();
            Serilog.Log.Information("Cache purge requested: removed {Count} solve-results / URL-data entries", removed);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { purged = removed });
        }

        static async Task GetInstalledPluginsGrasshopper(HttpContext ctx)
        {
            var ghPluginInfo = new SortedDictionary<string, string>();
            foreach (var obj in Grasshopper.Instances.ComponentServer.ObjectProxies.Where(o => o != null))
            {
                var asm = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(obj.Guid);
                if (asm != null && !string.IsNullOrEmpty(asm.Name) && !asm.IsCoreLibrary && !ghPluginInfo.ContainsKey(asm.Name))
                {
                    var version = (string.IsNullOrEmpty(asm.Version)) ? asm.Assembly.GetName().Version.ToString() : asm.Version;
                    ghPluginInfo.Add(asm.Name, version);
                }
            }
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(ghPluginInfo);
        }
    }
}

