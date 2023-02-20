using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GH_IO.Serialization;
using GH_IO.Types;
using Nancy;

namespace compute.geometry
{
    public class FixedEndPointsModule : NancyModule
    {
        public FixedEndPointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Get[""] = _ => HomePage(Context);
            Get["/healthcheck"] = _ => "healthy";
            Get["version"] = _ => GetVersion(Context);
            Get["servertime"] = _ => ServerTime(Context);
            Get["sdk/csharp"] = _ => CSharpSdk(Context);
            Get["plugins/rhino/installed"] = _ => GetInstalledPluginsRhino(Context);
            Get["plugins/gh/installed"] = _ => GetInstalledPluginsGrasshopper(Context);
        }

        static Response HomePage(NancyContext ctx)
        {
            return new Nancy.Responses.RedirectResponse("https://www.rhino3d.com/compute");
        }

        static Response GetVersion(NancyContext ctx)
        {
            var values = new Dictionary<string, string>();
            values.Add("rhino", Rhino.RhinoApp.Version.ToString());
            values.Add("compute", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            string git_sha = null; // appveyor will replace this
            values.Add("git_sha", git_sha);
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(values);
            response.ContentType = "application/json";
            return response;
        }

        static Response ServerTime(NancyContext ctx)
        {
            var response = (Nancy.Response)Newtonsoft.Json.JsonConvert.SerializeObject(DateTime.UtcNow);
            response.ContentType = "application/json";
            return response;
        }

        static Response CSharpSdk(NancyContext ctx)
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

        static Response GetInstalledPluginsRhino(NancyContext ctx)
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

            var response = (Response)Newtonsoft.Json.JsonConvert.SerializeObject(rhPluginInfo);
            response.ContentType = "application/json";
            return response;
        }

        static Response GetInstalledPluginsGrasshopper(NancyContext ctx)
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

            var response = (Response)Newtonsoft.Json.JsonConvert.SerializeObject(ghPluginInfo);
            response.ContentType = "application/json";
            return response;
        }
    }
}

