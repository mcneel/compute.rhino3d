using System;
using System.Collections.Generic;
using System.IO;
using BH.oM.RemoteCompute.RhinoCompute;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace compute.geometry
{
    static partial class DataCache
    {
        static string _definitionCacheDirectory;
        static string CacheDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_definitionCacheDirectory))
                {
                    string path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _definitionCacheDirectory = System.IO.Path.Combine(path, "McNeel", "rhino.compute", "definitioncache");
                }
                return _definitionCacheDirectory;
            }
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

        public static bool TryGetCachedDefinition(string key, out GrasshopperDefinition rc)
        {
            rc = null;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            // Check in-memory cache
            var cachedDef = System.Runtime.Caching.MemoryCache.Default.Get(key) as CachedDefinition;

            if (cachedDef != null)
            {
                rc = cachedDef.Definition;

                if (File.Exists(cachedDef.Definition.CacheKey))
                {
                    if (cachedDef.WatchedFileRuntimeSerialNumber != FileWatcher.WatchedFileRuntimeSerialNumber)
                    {
                        System.Runtime.Caching.MemoryCache.Default.Remove(key);
                    }
                }

                return true;
            }

            // Check file cache
            string filename = CacheFileName(key);

            if (filename != null && System.IO.File.Exists(filename))
            {
                try
                {
                    string data = System.IO.File.ReadAllText(filename);
                    rc = GrasshopperDefinitionUtils.FromBase64String(data);
                    return true;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"Unable to read cache file: {filename}");
                    Serilog.Log.Error(ex, "File error exception");
                }
            }

            return false;
        }

        public static bool CacheInMemory(string key, GrasshopperDefinition definition)
        {
            try
            {
                System.Runtime.Caching.MemoryCache.Default.Set(key, definition, CachePolicy);
                return true;
            }
            catch { }
            return false;
        }

        public static void CacheToDisk(string key, string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                string filename = CacheFileName(key);
                if (filename != null && !System.IO.File.Exists(filename))
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(CacheDirectory))
                            System.IO.Directory.CreateDirectory(CacheDirectory);

                        System.IO.File.WriteAllText(filename, data);
                        System.Threading.Tasks.Task.Run(() => DataCache.CGCacheDirectory());
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error($"Unable to write cache file: {filename}");
                        Serilog.Log.Error(ex, "File error exception");
                    }
                }
            }
        }

        public static string GetCachedSolveResults(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var cache = System.Runtime.Caching.MemoryCache.Default.Get(key) as CachedResults;
            if (cache == null)
                return null;

            if (File.Exists(cache.Definition.CacheKey))
            {
                if (cache.WatchedFileRuntimeSerialNumber != FileWatcher.WatchedFileRuntimeSerialNumber)
                {
                    System.Runtime.Caching.MemoryCache.Default.Remove(key);
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
                WatchedFileRuntimeSerialNumber = FileWatcher.WatchedFileRuntimeSerialNumber,
                Json = jsonResults
            };

            System.Runtime.Caching.MemoryCache.Default.Add(key, cache, CachePolicy);
        }

        public static object GetCachedItem(JToken token, Type objectType, JsonSerializer serializer)
        {
            string jsonString = token.ToString();
            if (jsonString.StartsWith("{") &&
                jsonString.EndsWith("}") &&
                jsonString.IndexOf("url", StringComparison.OrdinalIgnoreCase) > 0 &&
                jsonString.IndexOf("http", StringComparison.OrdinalIgnoreCase) > 0)
            {
                Dictionary<string, string> cacheDictionary = new Dictionary<string, string>();

                cacheDictionary = token.ToObject(cacheDictionary.GetType()) as Dictionary<string, string>;

                string url;

                if (cacheDictionary == null || !cacheDictionary.TryGetValue("url", out url))
                    return null;

                JToken jtoken = null;
                string key = $"url:{url.ToLower()}";
                Tuple<JToken, object> cacheEntry = System.Runtime.Caching.MemoryCache.Default.Get(key) as Tuple<JToken, object>;

                if (cacheEntry != null)
                {
                    Rhino.Geometry.GeometryBase geometry = cacheEntry.Item2 as Rhino.Geometry.GeometryBase;
                    if (geometry != null)
                        return geometry.DuplicateShallow();
                    jtoken = cacheEntry.Item1;
                }

                if (jtoken == null)
                {
                    using (var client = new System.Net.WebClient())
                    {
                        string cacheString = client.DownloadString(url);
                        object data = string.IsNullOrWhiteSpace(cacheString) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject(cacheString);
                        var ja = data as Newtonsoft.Json.Linq.JArray;
                        jtoken = ja[0];
                    }
                }

                if (jtoken != null)
                {
                    object rc = null;
                    if (serializer == null)
                        rc = jtoken.ToObject(objectType);
                    else
                        rc = jtoken.ToObject(objectType, serializer);

                    cacheEntry = new Tuple<JToken, object>(jtoken, rc);

                    System.Runtime.Caching.MemoryCache.Default.Add(key, cacheEntry, CachePolicy);

                    Rhino.Geometry.GeometryBase geometry = rc as Rhino.Geometry.GeometryBase;
                    if (geometry != null)
                        return geometry.DuplicateShallow();
                    return rc;
                }
            }
            return null;
        }

        //  we could do things like evict after 2 weeks with policy.SlidingExpiration = new TimeSpan(14, 0, 0, 0);
        private static System.Runtime.Caching.CacheItemPolicy CachePolicy { get; } = new System.Runtime.Caching.CacheItemPolicy();

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
                if (System.IO.Directory.Exists(CacheDirectory))
                {
                    var files = System.IO.Directory.GetFiles(CacheDirectory);
                    const int ALLOWED_COUNT = 20;
                    if (files != null && files.Length > ALLOWED_COUNT)
                    {
                        Array.Sort(files, CompareFilesByDate);
                        for (int i = ALLOWED_COUNT; i < files.Length; i++)
                        {
                            System.IO.File.Delete(files[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "exception while GC on cache directory");
            }
        }

        static string CacheFileName(string key)
        {
            return System.IO.Path.Combine(CacheDirectory, key + ".cache");
        }
    }
}
