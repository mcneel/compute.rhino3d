using System.Collections.Generic;
using Resthopper.IO;

namespace Hops
{
    /// <summary>
    /// Use a separate class for all memory caching. This allows us to control how
    /// caching is performed in the future and to clear a cache if we want.
    /// </summary>
    static class MemoryCache
    {
        static System.Runtime.Caching.MemoryCache _memCache = new System.Runtime.Caching.MemoryCache("HopsCache");
        public static Schema Get(string key)
        {
            var cachedResults = _memCache.Get(key) as Schema;
            return cachedResults;
        }

        public static void Set(string key, Schema schema)
        {
            _memCache.Set(key, schema, new System.Runtime.Caching.CacheItemPolicy());
        }

        public static void ClearCache()
        {
            _memCache.Dispose();
            _memCache = new System.Runtime.Caching.MemoryCache("HopsCache");
        }
    }
}
