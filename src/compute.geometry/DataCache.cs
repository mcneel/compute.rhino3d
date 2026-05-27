using System;
using System.Collections.Generic;
using GH_IO.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace compute.geometry
{
    static class DataCache
    {
        class CachedDefinition
        {
            public GrasshopperDefinition Definition{ get; set;}
            public uint WatchedFileRuntimeSerialNumber { get; set; }
        }

        class CachedResults
        {
            public GrasshopperDefinition Definition { get; set; }
            public uint WatchedFileRuntimeSerialNumber { get; set; }
            public string Json { get; set; }
        }

        public static string CreateCacheKey(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new System.Text.StringBuilder("md5_");
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        static bool LooksLikeACacheKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            if (!key.StartsWith("md5_", StringComparison.Ordinal))
                return false;
            if (key.Length > 100)
                return false;
            return true;
        }

        static string definitionCacheDirectory;
        static string DefinitionCacheDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(definitionCacheDirectory))
                {
                    string path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    definitionCacheDirectory = System.IO.Path.Combine(path, "McNeel", "rhino.compute", "definitioncache");
                }
                return definitionCacheDirectory;
            }
        }

        static int CompareFilesByDate(string a, string b)
        {
            var fileInfoA = new System.IO.FileInfo(a);
            var fileInfoB = new System.IO.FileInfo(b);
            return DateTime.Compare(fileInfoB.LastAccessTime, fileInfoA.LastAccessTime);
        }

        static void CGCacheDirectory()
        {
            try
            {
                if (System.IO.Directory.Exists(DefinitionCacheDirectory))
                {
                    var files = System.IO.Directory.GetFiles(DefinitionCacheDirectory);
                    const int ALLOWED_COUNT = 20;
                    if (files!=null && files.Length > ALLOWED_COUNT)
                    {
                        Array.Sort(files, CompareFilesByDate);
                        for(int i = ALLOWED_COUNT; i< files.Length; i++)
                        {
                            System.IO.File.Delete(files[i]);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex, "exception while GC on cache directory");
            }
        }

        static string DefinitionCacheFileName(string key)
        {
            if (LooksLikeACacheKey(key))
            {
                string filename = System.IO.Path.Combine(DefinitionCacheDirectory, key + ".cache");
                return filename;
            }
            return null;
        }

        public static GrasshopperDefinition GetCachedDefinition(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var def = definitionCache.Get(key) as CachedDefinition;
            if (def == null)
            {
                string filename = DefinitionCacheFileName(key);
                if (filename != null && System.IO.File.Exists(filename))
                {
                    try
                    {
                        string data = System.IO.File.ReadAllText(filename);
                        return GrasshopperDefinition.FromBase64String(data, true);
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"Unable to read cache file: {filename}");
                        Log.Error(ex, "File error exception");
                    }
                }
                return null;
            }
            if (def.Definition.IsLocalFileDefinition)
            {
                if(def.WatchedFileRuntimeSerialNumber != GrasshopperDefinition.WatchedFileRuntimeSerialNumber)
                {
                    definitionCache.Remove(key);
                    return null;
                }
            }
            return def.Definition;
        }

        public static void SetCachedDefinition(string key, GrasshopperDefinition definition, string data)
        {
            CachedDefinition cachedef = new CachedDefinition
            {
                Definition = definition,
                WatchedFileRuntimeSerialNumber = GrasshopperDefinition.WatchedFileRuntimeSerialNumber
            };
            definitionCache.Set(key, cachedef, CachePolicy);

            if (!string.IsNullOrWhiteSpace(data))
            {
                string filename = DefinitionCacheFileName(key);
                if (filename != null && !System.IO.File.Exists(filename))
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(DefinitionCacheDirectory))
                            System.IO.Directory.CreateDirectory(DefinitionCacheDirectory);

                        System.IO.File.WriteAllText(filename, data);
                        System.Threading.Tasks.Task.Run(() => DataCache.CGCacheDirectory());
                    }
                    catch(Exception ex)
                    {
                        Log.Error($"Unable to write cache file: {filename}");
                        Log.Error(ex, "File error exception");
                    }
                }
            }
        }

        public static string GetCachedSolveResults(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var cache = resultsCache.Get(key) as CachedResults;
            if (cache == null)
                return null;

            if (cache.Definition.IsLocalFileDefinition)
            {
                if (cache.WatchedFileRuntimeSerialNumber != GrasshopperDefinition.WatchedFileRuntimeSerialNumber)
                {
                    resultsCache.Remove(key);
                    return null;
                }
            }

            return cache.Json;
        }

        public static void SetCachedSolveResults(string key, string jsonResults, GrasshopperDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var cache = new CachedResults
            {
                Definition = definition,
                WatchedFileRuntimeSerialNumber = GrasshopperDefinition.WatchedFileRuntimeSerialNumber,
                Json = jsonResults
            };

            resultsCache.Add(key, cache, CachePolicy);
        }

        public static object GetCachedItem(JToken token, Type objectType, JsonSerializer serializer)
        {
            string jsonString = token.ToString();
            if (jsonString.StartsWith("{") &&
                jsonString.EndsWith("}") &&
                jsonString.IndexOf("url", StringComparison.OrdinalIgnoreCase)>0 &&
                jsonString.IndexOf("http", StringComparison.OrdinalIgnoreCase)>0)
            {
                Dictionary<string, string> cacheDictionary = new Dictionary<string, string>();
                cacheDictionary = token.ToObject(cacheDictionary.GetType()) as Dictionary<string, string>;
                string url;
                if (cacheDictionary == null || !cacheDictionary.TryGetValue("url", out url))
                    return null;

                JToken jtoken = null;
                string key = $"url:{url.ToLower()}";
                Tuple<JToken, object> cacheEntry = resultsCache.Get(key) as Tuple<JToken, object>;
                if( cacheEntry!=null)
                {
                    Rhino.Geometry.GeometryBase geometry = cacheEntry.Item2 as Rhino.Geometry.GeometryBase;
                    if (geometry != null)
                        return geometry.DuplicateShallow();
                    jtoken = cacheEntry.Item1;
                }
                if (jtoken == null)
                {
                    // UrlGuard adds scheme validation, optional private-IP blocking,
                    // and a Config.MaxRequestSize cap to the outbound fetch.
                    string cacheString = UrlGuard.GetStringAsync(url, Config.MaxRequestSize).Result;
                    object data = string.IsNullOrWhiteSpace(cacheString) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject(cacheString);
                    // Expect the URL to return a non-empty JSON array. Throw a descriptive
                    // error instead of a bare NullReferenceException/IndexOutOfRange when the
                    // fetched content is the wrong shape (this is an external-data boundary).
                    if (!(data is Newtonsoft.Json.Linq.JArray ja) || ja.Count == 0)
                        throw new InvalidOperationException($"Expected URL '{url}' to return a non-empty JSON array.");
                    jtoken = ja[0];
                }

                if( jtoken!=null )
                {
                    object rc = null;
                    if (serializer == null)
                        rc = jtoken.ToObject(objectType);
                    else
                        rc = jtoken.ToObject(objectType, serializer);

                    cacheEntry = new Tuple<JToken, object>(jtoken, rc);
                    resultsCache.Add(key, cacheEntry, CachePolicy);
                    Rhino.Geometry.GeometryBase geometry = rc as Rhino.Geometry.GeometryBase;
                    if (geometry != null)
                        return geometry.DuplicateShallow();
                    return rc;
                }
            }
            return null;
        }

        private static System.Runtime.Caching.CacheItemPolicy CachePolicy
        {
            get
            {
                var policy = new System.Runtime.Caching.CacheItemPolicy();
                // no policy yet, but we could do things like evict after 2 weeks with
                //policy.SlidingExpiration = new TimeSpan(14, 0, 0, 0);
                return policy;
            }
        }

        // Two separate MemoryCache instances with deliberately different eviction policies:
        //
        //  - definitionCache: stores parsed GrasshopperDefinitions. NEVER evicts under
        //    memory pressure. Clients send a /io request, receive a Pointer in the
        //    response, then reference that Pointer in subsequent /grasshopper calls so
        //    they don't have to re-upload the full definition. Evicting an entry here
        //    breaks every pointer-based client holding that Pointer.
        //
        //  - resultsCache: stores serialized solve outputs AND URL-fetched JSON data.
        //    LRU-evicts under host memory pressure (configured via
        //    PhysicalMemoryLimitPercentage = Config.CachePhysicalLimitPercent, default 70%).
        //    On cache miss, both kinds of entry are re-derivable: a missing solve result
        //    just triggers another solve (slower but correct), and a missing URL fetch
        //    re-pulls from the source via UrlGuard.
        //
        // PollingInterval controls how often the cache checks host memory. 30 seconds is
        // a balance — fast enough to keep up with bursty allocations from Rhino/Grasshopper,
        // slow enough to avoid pure-syscall overhead.
        static readonly System.Runtime.Caching.MemoryCache definitionCache =
            new System.Runtime.Caching.MemoryCache("compute.geometry.definitions");
        static readonly System.Runtime.Caching.MemoryCache resultsCache =
            new System.Runtime.Caching.MemoryCache("compute.geometry.results",
                new System.Collections.Specialized.NameValueCollection
                {
                    { "PhysicalMemoryLimitPercentage", Config.CachePhysicalLimitPercent.ToString() },
                    { "PollingInterval", "00:00:30" },
                });

        /// <summary>
        /// Purges every entry from the solve-results / URL-data cache. Does NOT touch the
        /// definition cache, since active clients may hold Pointer references to entries
        /// there. Returns the number of items removed. Exposed via the POST /cache/purge
        /// endpoint for operator-driven cleanup when host memory pressure is observed.
        /// </summary>
        public static long PurgeSolveResults()
        {
            return resultsCache.Trim(100);
        }
    }
}
