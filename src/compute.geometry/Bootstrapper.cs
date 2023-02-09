using System;

/*
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Serilog;

namespace compute.geometry
{
    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private byte[] _favicon;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            // 7.5 will have direct sending of RhionApp.Write to the console. Until we
            // have stabilized on 7.5, look for the property using reflection
            var sendToConsole = typeof(Rhino.RhinoApp).GetProperty("SendWriteToConsole");

            if (sendToConsole != null)
                sendToConsole.SetValue(null, true);
            else
                Rhino.RhinoApp.CommandWindowCaptureEnabled = true;
            // Load GH at startup so it can get initialized on the main thread
            Log.Information("(1/2) Loading grasshopper");
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
            if (runheadless != null)
                runheadless.Invoke(pluginObject, null);


            if (sendToConsole != null)
                sendToConsole.SetValue(null, false);
            else
            {
                var lines = Rhino.RhinoApp.CapturedCommandWindowStrings(true);
                if (lines != null && lines.Length > 0)
                {
                    foreach (var line in lines)
                    {
                        Log.Information(line.Trim());
                    }
                }
                Rhino.RhinoApp.CommandWindowCaptureEnabled = false;
            }
            Log.Information("(2/2) Loading compute plug-ins");
            var loadComputePlugins = typeof(Rhino.PlugIns.PlugIn).GetMethod("LoadComputeExtensionPlugins");
            if (loadComputePlugins != null)
                loadComputePlugins.Invoke(null, null);

            Nancy.StaticConfiguration.DisableErrorTraces = false;

            pipelines.OnError += LogError;
            ApiKey.Initialize(pipelines);

            Rhino.Runtime.HostUtils.RegisterComputeEndpoint("grasshopper", typeof(Endpoints.GrasshopperEndpoint));

            base.ApplicationStartup(container, pipelines);
        }

        protected override IEnumerable<Func<Assembly, bool>> AutoRegisterIgnoredAssemblies =>
            base.AutoRegisterIgnoredAssemblies.Union(new Func<Assembly, bool>[]
            {
                // ignore these assemblies when autoregistering the "application container"
                asm => asm.FullName.StartsWith("Rhino", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("Eto", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("Grasshopper", StringComparison.Ordinal),
            });

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("docs"));
        }

        protected override byte[] FavIcon => _favicon ?? (_favicon = LoadFavIcon());

        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("compute.geometry.favicon.ico"))
            {
                var memoryStream = new System.IO.MemoryStream();
                resourceStream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }

        private static dynamic LogError(NancyContext ctx, Exception ex)
        {
            string id = ctx.Request.Headers["X-Compute-Id"].FirstOrDefault(); // set by frontend (ignore)
            var msg = "An exception occured while processing request";
            if (id != null)
                Log.Error(ex, msg + " \"{RequestId}\"", id);
            else
                Log.Error(ex, msg);
            return null;
        }
    }
}
*/
