using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace compute.geometry
{
    static class Config
    {
        /// <summary>
        /// RHINO_COMPUTE_URLS: the list of URLs that compute will listen on.
        /// </summary>
        public static string[] Urls { get; set; }

        /// <summary>
        /// Localhost port to use. This is only used on localhost when Urls is empty
        /// </summary>
        public static int LocalhostPort { get; set; }

        /// <summary>
        /// RHINO_COMPUTE_KEY: the API key required to make POST requests.
        /// Leave empty to disable.
        /// </summary>
        public static string ApiKey { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_TIMEOUT: time in seconds for outbound HTTP fetches performed by this
        /// process (e.g. downloading a Grasshopper definition from a URL via UrlGuard /
        /// HttpClientHelper). Defaults to 100 seconds — matches the .NET HttpClient default
        /// and rhino.compute's reverse-proxy timeout, so the same env var configures both
        /// processes via a single knob. Spawned compute.geometry children automatically
        /// inherit the value rhino.compute sets when launched with --timeout.
        /// </summary>
        public static int RequestTimeout { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_MAX_REQUEST_SIZE: maximum allowed size of any request body in bytes.
        /// Defaults to 50 MB. Matches the rhino.compute env-var pattern so child processes
        /// inherit the same limit when launched under the proxy. Also used as the cap for
        /// server-side URL fetches in UrlGuard.
        /// </summary>
        public static long MaxRequestSize { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_CACHE_PHYSICAL_LIMIT_PERCENT: percentage of the host's physical
        /// memory at which the solve-results cache (and URL-fetched data cache) begins
        /// LRU-evicting entries. Defaults to 70. Definitions are cached in a separate
        /// instance with no eviction so pointer-based clients (the /io → pointer →
        /// /grasshopper flow) don't break when memory pressure rises.
        /// </summary>
        public static int CachePhysicalLimitPercent { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_BLOCK_PRIVATE_URLS: when true, refuses server-side URL fetches
        /// whose hostname DNS-resolves to a private, loopback, or link-local IP. Defaults
        /// to false to preserve backward compatibility for deployments that legitimately
        /// fetch from internal hosts (intranet file servers, VPC endpoints, etc.).
        ///
        /// Recommended for any deployment that has a public IP — especially cloud VMs,
        /// where this protects against SSRF attacks targeting the cloud metadata endpoint
        /// at 169.254.169.254 (which exposes IAM credentials on AWS/Azure/GCP).
        ///
        /// rhino.compute exposes this via --block-private-urls in its CLI; setting the
        /// flag on the parent propagates the env var to spawned compute.geometry children.
        /// </summary>
        public static bool BlockPrivateUrls { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_LOG_PATH: the directory in which to write logs.
        /// </summary>
        public static string LogPath { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_LOG_RETAIN_DAYS: the number of days worth of logs to retain.
        /// Files are rotated daily.
        /// </summary>
        public static int LogRetainDays { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_CREATE_HEADLESS_DOC: create a headless Rhino document for each request.
        /// </summary>
        public static bool CreateHeadlessDoc { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_LOAD_GRASSHOPPER: load Grasshopper plugin at startup (defaults to true).
        /// Set to false to skip Grasshopper loading for faster startup when only using geometry endpoints.
        /// </summary>
        public static bool LoadGrasshopper { get; private set; }

        public static string[] GetDeprecationWarnings() => warnings.ToArray();

        /// <summary>
        /// RHINO_COMPUTE_DEBUG: enables debug logging (defaults to true in DEBUG).
        /// </summary>
        public static bool Debug { get; private set; }

        /// <summary>
        /// Loads config from environment variables (or uses defaults).
        /// </summary>
        public static void Load()
        {
            Urls = GetEnvironmentVariable(RHINO_COMPUTE_URLS, "http://localhost:8081", COMPUTE_BIND_URLS).Split(';');
            ApiKey = GetEnvironmentVariable<string>(RHINO_COMPUTE_KEY, null);
            RequestTimeout = GetEnvironmentVariable<int>(RHINO_COMPUTE_TIMEOUT, 100);
            MaxRequestSize = GetEnvironmentVariable<long>(RHINO_COMPUTE_MAX_REQUEST_SIZE, 52428800);
            BlockPrivateUrls = GetEnvironmentVariable<bool>(RHINO_COMPUTE_BLOCK_PRIVATE_URLS, false);
            CachePhysicalLimitPercent = GetEnvironmentVariable<int>(RHINO_COMPUTE_CACHE_PHYSICAL_LIMIT_PERCENT, 70);
            LogPath = GetEnvironmentVariable(RHINO_COMPUTE_LOG_PATH, Path.Combine(Path.GetTempPath(), "Compute", "Logs"), COMPUTE_LOG_PATH);
            LogRetainDays = GetEnvironmentVariable(RHINO_COMPUTE_LOG_RETAIN_DAYS, 10, COMPUTE_LOG_RETAIN_DAYS);
            CreateHeadlessDoc = GetEnvironmentVariable<bool>(RHINO_COMPUTE_CREATE_HEADLESS_DOC, false);
            LoadGrasshopper = GetEnvironmentVariable<bool>(RHINO_COMPUTE_LOAD_GRASSHOPPER, true);

#if DEBUG
            Debug = true;
#elif RELEASE
            Debug = false;
#endif
            Debug = GetEnvironmentVariable(RHINO_COMPUTE_DEBUG, Debug);

            foreach (var name in ignored)
            {
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
                    warnings.Add($"Ignoring deprecated {name} environment variable");
            }
        }

        #region private

        // environment variables
        const string RHINO_COMPUTE_URLS = "RHINO_COMPUTE_URLS";
        const string RHINO_COMPUTE_KEY = "RHINO_COMPUTE_KEY";
        const string RHINO_COMPUTE_TIMEOUT = "RHINO_COMPUTE_TIMEOUT";
        const string RHINO_COMPUTE_MAX_REQUEST_SIZE = "RHINO_COMPUTE_MAX_REQUEST_SIZE";
        const string RHINO_COMPUTE_BLOCK_PRIVATE_URLS = "RHINO_COMPUTE_BLOCK_PRIVATE_URLS";
        const string RHINO_COMPUTE_CACHE_PHYSICAL_LIMIT_PERCENT = "RHINO_COMPUTE_CACHE_PHYSICAL_LIMIT_PERCENT";
        const string RHINO_COMPUTE_LOG_PATH = "RHINO_COMPUTE_LOG_PATH";
        const string RHINO_COMPUTE_LOG_RETAIN_DAYS = "RHINO_COMPUTE_LOG_RETAIN_DAYS";
        const string RHINO_COMPUTE_DEBUG = "RHINO_COMPUTE_DEBUG";
        const string RHINO_COMPUTE_CREATE_HEADLESS_DOC = "RHINO_COMPUTE_CREATE_HEADLESS_DOC";
        const string RHINO_COMPUTE_LOAD_GRASSHOPPER = "RHINO_COMPUTE_LOAD_GRASSHOPPER";

        // deprecated
        const string COMPUTE_BIND_URLS = "COMPUTE_BIND_URLS";
        const string COMPUTE_LOG_PATH = "COMPUTE_LOG_PATH";
        const string COMPUTE_LOG_RETAIN_DAYS = "COMPUTE_LOG_RETAIN_DAYS";

        readonly static string[] ignored = new string[] { "COMPUTE_BACKEND_PORT" };

        readonly static List<string> warnings = new List<string>();

        static T GetEnvironmentVariable<T>(string name, T defaultValue, string deprecatedName = null)
        {
            string value = Environment.GetEnvironmentVariable(name);
            
            if (string.IsNullOrWhiteSpace(value) && deprecatedName != null)
            {
                value = Environment.GetEnvironmentVariable(deprecatedName);
                if (!string.IsNullOrWhiteSpace(value))
                    warnings.Add($"{deprecatedName} is deprecated; use {name} instead");
            }

            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (typeof(T) == typeof(bool))
            {
                if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return (T)(object)true;
                return (T)(object)false;
            }
            
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(value, out int result))
                    return (T)(object)result;

                warnings.Add($"{name} set to '{value}'; unable to parse as integer");
                return defaultValue;
            }

            if (typeof(T) == typeof(long))
            {
                if (long.TryParse(value, out long result))
                    return (T)(object)result;

                warnings.Add($"{name} set to '{value}'; unable to parse as long");
                return defaultValue;
            }

            return (T)(object)value;
        }

        #endregion
    }
}

