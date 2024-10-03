using System;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            BH.Engine.Computing.Log.Clean();

            Post["/grasshopper"] = _ => GrasshopperEndpoint(Context);
            Post["/solve/base64"] = _ => SolveBase64(Context);
            Post["/solve/cached"] = _ => SolveCache(Context);
            Post["/solve/url"] = _ => SolveUrl(Context);
            Post["/io/base64"] = _ => IoBase64(Context);
            Post["/io/url"] = _ => IoUrl(Context);
            Get["/io/cached/{cacheKey}"] = x => IoCacheKey(Context, x.cacheKey);
            Get["/isCached/{cacheKey}"] = x => IsCached(Context, x.cacheKey);
        }
    }
}
