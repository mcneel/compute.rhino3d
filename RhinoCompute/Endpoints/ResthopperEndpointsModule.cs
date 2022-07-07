using Nancy;
using Nancy.Extensions;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            BH.Engine.RemoteCompute.Log.Clean();

            Post["/grasshopper"] = _ => GrasshopperEndpoint(Context);
            Post["/io"] = _ => IOsEndpoint(Context, true);
            Get["/io"] = _ => IOsEndpoint(Context, false);
        }

        static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        static object _ghsolvelock = new object();
    }

    public static class NancyExtensions
    {
        public static string GetBody(this NancyContext ctx)
        {
            string body = ctx.Request.Body.AsString();
            if (body.StartsWith("[") && body.EndsWith("]"))
                body = body.Substring(1, body.Length - 2);

            return body;
        }
    }
}
