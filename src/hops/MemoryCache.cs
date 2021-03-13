using Resthopper.IO;

namespace Hops
{
    /// <summary>
    /// Use a separate class for all memory caching. This allows us to control how
    /// caching is performed in the future and to clear a cache if we want.
    /// </summary>
    static class MemoryCache
    {
        public static Schema Get(string key)
        {
            var cachedResults = System.Runtime.Caching.MemoryCache.Default.Get(key) as Schema;
            return cachedResults;
        }

        public static void Set(string key, Schema schema)
        {
            System.Runtime.Caching.MemoryCache.Default.Set(key, schema, new System.Runtime.Caching.CacheItemPolicy());
        }
    }
}
