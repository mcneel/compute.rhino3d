namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Post["/grasshopper"] = _ => GrasshopperEndpoint(Context);
            Post["/io"] = _ => IOsEndpoint(Context, true);
            Get["/io"] = _ => IOsEndpoint(Context, false);
        }

        static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        static object _ghsolvelock = new object();
    }
}
