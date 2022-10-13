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

        public static GrasshopperDefinition GrasshopperDefinitionFromCacheKey(string cacheKey)
        {
            GrasshopperDefinition rc = null;
            if (!DataCache.TryGetCachedDefinition(cacheKey, out rc))
            {
                Log.RecordError($"Could not fetch the Grasshopper definition from cache at input key `{cacheKey}`.");
                return rc;
            }

            return rc;
        }

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
                rc = GrasshopperDefinitionFromCacheKey(data);
                return true;
            }
            catch (Exception ex)
            {
                Log.RecordError($"Unable to read cache file: {filepath}");
                Log.RecordError(ex, "File error exception");
            }

            return false;
        }

        /// <summary>
        /// Cache a GrasshopperDefinition in memory.
        /// </summary>
        /// <param name="cacheKey">The cache key under which the definition is stored.</param>
        /// <param name="base64definition">Provide it if the serialized definition is already available to speed up the caching.</param>
        /// <returns></returns>
        public static bool TryWriteInMemory(GrasshopperDefinition definition, out string cacheKey, string base64definition = null)
        {
            if (string.IsNullOrEmpty(base64definition))
                base64definition = definition.ToBase64String();

            cacheKey = base64definition.CacheKey();

            try
            {
                System.Runtime.Caching.MemoryCache.Default.Set(cacheKey, definition, CachePolicy);
                return true;
            }
            catch { }

            return false;
        }

        public static bool TryWriteToDisk(GrasshopperDefinition definition, out string cacheKey)
        {
            return TryWriteToDisk(definition.ToBase64String(), out cacheKey);
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
