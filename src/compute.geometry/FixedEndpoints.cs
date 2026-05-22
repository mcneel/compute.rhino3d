using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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

