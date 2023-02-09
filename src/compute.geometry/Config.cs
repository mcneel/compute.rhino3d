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
        /// RHINO_COMPUTE_LOG_PATH: the directory in which to write logs.
        /// </summary>
        public static string LogPath { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_LOG_RETAIN_DAYS: the number of days worth of logs to retain.
        /// Files are rotated daily.
        /// </summary>
        public static int LogRetainDays { get; private set; }

        public static string[] GetDeprecationWarnings() => _warnings.ToArray();

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
            LogPath = GetEnvironmentVariable(RHINO_COMPUTE_LOG_PATH, Path.Combine(Path.GetTempPath(), "Compute", "Logs"), COMPUTE_LOG_PATH);
            LogRetainDays = GetEnvironmentVariable(RHINO_COMPUTE_LOG_RETAIN_DAYS, 10, COMPUTE_LOG_RETAIN_DAYS);

#if DEBUG
            Debug = true;
#endif
            Debug = GetEnvironmentVariable(RHINO_COMPUTE_DEBUG, Debug);

            foreach (var name in _ignored)
            {
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
                    _warnings.Add($"Ignoring deprecated {name} environment variable");
            }
        }

        #region private

        // environment variables
        const string RHINO_COMPUTE_URLS = "RHINO_COMPUTE_URLS";
        const string RHINO_COMPUTE_KEY = "RHINO_COMPUTE_KEY";
        const string RHINO_COMPUTE_LOG_PATH = "RHINO_COMPUTE_LOG_PATH";
        const string RHINO_COMPUTE_LOG_RETAIN_DAYS = "RHINO_COMPUTE_LOG_RETAIN_DAYS";
        const string RHINO_COMPUTE_DEBUG = "RHINO_COMPUTE_DEBUG";

        // deprecated
        const string COMPUTE_BIND_URLS = "COMPUTE_BIND_URLS";
        const string COMPUTE_LOG_PATH = "COMPUTE_LOG_PATH";
        const string COMPUTE_LOG_RETAIN_DAYS = "COMPUTE_LOG_RETAIN_DAYS";

        readonly static string[] _ignored = new string[] { "COMPUTE_BACKEND_PORT" };

        readonly static List<string> _warnings = new List<string>();

        static T GetEnvironmentVariable<T>(string name, T defaultValue, string deprecatedName = null)
        {
            string value = Environment.GetEnvironmentVariable(name);
            
            if (string.IsNullOrWhiteSpace(value) && deprecatedName != null)
            {
                value = Environment.GetEnvironmentVariable(deprecatedName);
                if (!string.IsNullOrWhiteSpace(value))
                    _warnings.Add($"{deprecatedName} is deprecated; use {name} instead");
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

                _warnings.Add($"{name} set to '{value}'; unable to parse as integer");
                return defaultValue;
            }

            return (T)(object)value;
        }

        #endregion
    }
}

