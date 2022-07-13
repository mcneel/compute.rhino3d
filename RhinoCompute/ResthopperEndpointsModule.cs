using System;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            BH.Engine.RemoteCompute.Log.Clean();

            Post["/grasshopper"] = _ => GrasshopperEndpoint(Context);
            Post["/solve/base64"] = _ => SolveBase64(Context);
            Post["/solve/cached"] = _ => SolveCache(Context);
            Post["/solve/url"] = _ => SolveUrl(Context);
            Post["/io/base64"] = _ => IoBase64(Context);
            Post["/io/url"] = _ => IoUrl(Context);
            Post["/io/cached"] = _ => IoCacheKey(Context);
            Get["/isCached"] = _ => throw new NotImplementedException();
        }
    }
}
