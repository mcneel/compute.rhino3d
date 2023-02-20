using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.PlugIns;

namespace compute.geometry
{
    public class FixedEndPointsModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("", HomePage);
            app.MapGet("version", GetVersion);
            app.MapGet("servertime", ServerTime);
            app.MapGet("plugins/rhino/installed", GetInstalledPluginsRhino);
            app.MapGet("plugins/gh/installed", GetInstalledPluginsGrasshopper);
            //Get["sdk/csharp"] = _ => CSharpSdk(Context);
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
                if (info != null && !info.ShipsWithRhino && !rhPluginInfo.ContainsKey(info.Name))
                {
                    rhPluginInfo.Add(info.Name, info.Version);
                }
            }

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(rhPluginInfo);
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

        /*
        static async Task CSharpSdk(HttpContext ctx)
        {
            string content = "";
            using (var resourceStream = typeof(FixedEndPointsModule).Assembly.GetManifestResourceStream("compute.geometry.RhinoCompute.cs"))
            {
                var stream = new System.IO.StreamReader(resourceStream);
                content = stream.ReadToEnd();
                stream.Close();
            }

            var response = new Response();

            response.Headers.Add("Content-Disposition", "attachment; filename=RhinoCompute.cs");
            response.ContentType = "text/plain";
            response.Contents = stream => {
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    writer.Write(content);
                }
            };
            return response.AsAttachment("RhinoCompute.cs", "text/plain" );
        }
        */
    }
}

