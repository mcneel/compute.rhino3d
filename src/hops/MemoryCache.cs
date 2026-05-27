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
        static System.Runtime.Caching.MemoryCache memoryCache = new System.Runtime.Caching.MemoryCache("HopsCache");

        public static Schema Get(string key)
        {
            var cachedResults = memoryCache.Get(key) as Schema;
            return cachedResults;
        }

        public static void Set(string key, Schema schema)
        {
            EntryCount++;
            memoryCache.Set(key, schema, new System.Runtime.Caching.CacheItemPolicy());
        }

        public static void ClearCache()
        {
            memoryCache.Dispose();
            memoryCache = new System.Runtime.Caching.MemoryCache("HopsCache");
            EntryCount = 0;
        }

        public static int EntryCount { get; set; } = 0;
    }
}
