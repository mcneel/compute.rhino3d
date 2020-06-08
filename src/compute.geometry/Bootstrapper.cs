using System;
using System.Linq;
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
            Log.Information("Launching RhinoCore library as {User}", Environment.UserName);
            Program.RhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            // Load GH at startup so it can get initialized on the main thread
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            var runheadless = pluginObject?.GetType().GetMethod("RunHeadless");
            if (runheadless != null)
                runheadless.Invoke(pluginObject, null);

            var loadComputePlugsin = typeof(Rhino.PlugIns.PlugIn).GetMethod("LoadComputeExtensionPlugins");
            if (loadComputePlugsin != null)
                loadComputePlugsin.Invoke(null, null);

            Nancy.StaticConfiguration.DisableErrorTraces = false;
            pipelines.OnError += LogError;
            ApiKey.Initialize(pipelines);


            Rhino.Runtime.HostUtils.RegisterComputeEndpoint("grasshopper", typeof(Endpoints.GrasshopperEndpoint));

            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("docs"));
        }

        protected override byte[] FavIcon
        {
            get { return _favicon ?? (_favicon = LoadFavIcon()); }
        }

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
            string id = ctx.Request.Headers["X-Compute-Id"].FirstOrDefault(); // set by frontend
            var msg = "An exception occured while processing request";
            if (id != null)
                Log.Error(ex, msg + " \"{RequestId}\"", id);
            else
                Log.Error(ex, msg);
            return null;
        }
    }
}
