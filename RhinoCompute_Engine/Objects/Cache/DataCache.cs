using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BH.Engine.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class DataCache
    {
        private static string m_cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BHoM", "RhinoCompute", "definitioncache");

        public static bool TryGetCachedDefinition(string key, out GrasshopperDefinition rc)
        {
            rc = null;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            // Check in-memory cache
            CachedDefinition cachedDef = System.Runtime.Caching.MemoryCache.Default.Get(key) as CachedDefinition;

            if (cachedDef != null)
            {
                rc = cachedDef.Definition;
                return true;
            }

            // Check file cache
            string filepath = key.CacheFilePath();

            if (!System.IO.File.Exists(filepath))
                return false;

            try
            {
                string data = System.IO.File.ReadAllText(filepath);
                rc = Create.GrasshopperDefinitionFromCacheKey(data);
                return true;
            }
            catch (Exception ex)
            {
                Log.RecordError($"Unable to read cache file: {filepath}");
                Log.RecordError(ex, "File error exception");
            }

            return false;
        }

        public static bool WriteInMemory(GrasshopperDefinition definition, string base64definition = null)
        {
            if (string.IsNullOrEmpty(base64definition))
                base64definition = definition.ToBase64String();

            string cacheKey = base64definition.CacheKey();

            try
            {
                System.Runtime.Caching.MemoryCache.Default.Set(cacheKey, definition, CachePolicy);
                return true;
            }
            catch { }

            return false;
        }

        public static bool TryWriteToDisk(string base64script, out string cacheKey)
        {
            cacheKey = base64script.CacheKey();

            if (string.IsNullOrWhiteSpace(cacheKey) || string.IsNullOrWhiteSpace(base64script))
                return false;

            try
            {
                if (!System.IO.Directory.Exists(m_cacheDirectory))
                    System.IO.Directory.CreateDirectory(m_cacheDirectory);

                System.IO.File.WriteAllText(cacheKey.CacheFilePath(), base64script);
                System.Threading.Tasks.Task.Run(() => CGCacheDirectory());

                return true;
            }
            catch (Exception ex)
            {
                Log.RecordError($"Unable to write cache file: {cacheKey.CacheFilePath()}");
                Log.RecordError(ex, "File error exception");
            }

            return false;
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

        private static System.Runtime.Caching.CacheItemPolicy CachePolicy { get; } = new System.Runtime.Caching.CacheItemPolicy();

        private static int CompareFilesByDate(string a, string b)
        {
            var fileInfoA = new System.IO.FileInfo(a);
            var fileInfoB = new System.IO.FileInfo(b);
            return DateTime.Compare(fileInfoB.LastAccessTime, fileInfoA.LastAccessTime);
        }

        private static void CGCacheDirectory()
        {
            try
            {
                if (System.IO.Directory.Exists(m_cacheDirectory))
                {
                    var files = System.IO.Directory.GetFiles(m_cacheDirectory);
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
                Log.RecordError(ex, "exception while GC on cache directory");
            }
        }

        private static string CacheFilePath(this string key)
        {
            return Path.Combine(m_cacheDirectory, key);
        }
    }
}
