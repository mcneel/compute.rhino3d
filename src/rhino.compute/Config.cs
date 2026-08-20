using System;
using System.Collections.Generic;
using System.IO;

namespace rhino.compute
{
    static class Config
    {
        /// <summary>
        /// RHINO_COMPUTE_KEY: the API key required to make POST requests.
        /// Leave empty to disable.
        /// </summary>
        public static string ApiKey { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_TIMEOUT: time in seconds for a time out from the client
        /// </summary>
        public static int ReverseProxyRequestTimeout { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_REQUEST_LIMIT: maximum allowed size of any request body in bytes.
        /// </summary>
        public static long MaxRequestSize { get; private set; }

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
        /// RHINO_COMPUTE_DEBUG: enables debug logging (defaults to true in DEBUG).
        /// </summary>
        public static bool Debug { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT: seconds to wait for a newly spawned
        /// compute.geometry child to open its port before giving up on it.
        ///
        /// A child does not bind its port until Rhino, Grasshopper and the compute
        /// plug-ins have all finished loading — compute.geometry's Startup.Configure
        /// calls RhinoCoreStartup() synchronously, so the whole Rhino boot happens
        /// before Kestrel listens. On a cold cloud instance (empty file cache, first
        /// Grasshopper library scan, scripting environment init) that regularly takes
        /// longer than the default, which makes the very first request to a freshly
        /// booted server fail while the second one succeeds off a warm cache.
        ///
        /// The 60s default is historical: it was raised to 60 in March 2021 (PR #241)
        /// for children launched locally by Hops on a developer workstation, and has
        /// not been revisited since. Raise it wherever a slow first call is preferable
        /// to a failed one.
        /// </summary>
        public static int ChildStartupTimeout { get; private set; }

        /// <summary>
        /// Default for <see cref="ChildStartupTimeout"/>. Public so callers can tell
        /// whether the running value was tuned or left alone.
        /// </summary>
        public const int DefaultChildStartupTimeout = 60;

        /// <summary>
        /// Non-fatal configuration problems collected during <see cref="Load"/> —
        /// unparseable values, out-of-range values, deprecated names. Load() runs
        /// before the logger exists, so these are buffered here for the caller to
        /// emit once logging is up.
        /// </summary>
        public static IReadOnlyList<string> Warnings => warnings;

        /// <summary>
        /// Loads config from environment variables (or uses defaults).
        /// </summary>
        public static void Load()
        {
            ApiKey = GetEnvironmentVariable<string>(RHINO_COMPUTE_KEY, null);
            ReverseProxyRequestTimeout = GetEnvironmentVariable<int>(RHINO_COMPUTE_TIMEOUT, 100);
            MaxRequestSize = GetEnvironmentVariable<long>(RHINO_COMPUTE_MAX_REQUEST_SIZE, 52428800);
            LogPath = GetEnvironmentVariable(RHINO_COMPUTE_LOG_PATH, Path.Combine(Path.GetTempPath(), "Compute", "Logs"));
            LogRetainDays = GetEnvironmentVariable(RHINO_COMPUTE_LOG_RETAIN_DAYS, 10);

            ChildStartupTimeout = GetEnvironmentVariable(RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT, DefaultChildStartupTimeout);
            // Clamp rather than honour a nonsensical value: too low makes every spawn
            // fail before Rhino can possibly be ready, too high leaves callers blocked
            // for hours on a child that is never coming up.
            if (ChildStartupTimeout < MinChildStartupTimeout || ChildStartupTimeout > MaxChildStartupTimeout)
            {
                warnings.Add($"{RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT} set to '{ChildStartupTimeout}'; " +
                             $"outside the supported range {MinChildStartupTimeout}-{MaxChildStartupTimeout} seconds. " +
                             $"Using the default of {DefaultChildStartupTimeout}.");
                ChildStartupTimeout = DefaultChildStartupTimeout;
            }

#if DEBUG
            Debug = true;
#elif RELEASE
            Debug = false;
#endif
            Debug = GetEnvironmentVariable(RHINO_COMPUTE_DEBUG, Debug);
        }

        #region private
        // environment variables
        const string RHINO_COMPUTE_KEY = "RHINO_COMPUTE_KEY";
        const string RHINO_COMPUTE_TIMEOUT = "RHINO_COMPUTE_TIMEOUT";
        const string RHINO_COMPUTE_MAX_REQUEST_SIZE = "RHINO_COMPUTE_MAX_REQUEST_SIZE";
        const string RHINO_COMPUTE_LOG_PATH = "RHINO_COMPUTE_LOG_PATH";
        const string RHINO_COMPUTE_LOG_RETAIN_DAYS = "RHINO_COMPUTE_LOG_RETAIN_DAYS";
        const string RHINO_COMPUTE_DEBUG = "RHINO_COMPUTE_DEBUG";
        const string RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT = "RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT";

        // Bounds for ChildStartupTimeout. The floor is well below any realistic Rhino
        // load time and exists only to reject 0/negative; the ceiling is an hour, past
        // which a stuck child should be diagnosed rather than waited on.
        const int MinChildStartupTimeout = 5;
        const int MaxChildStartupTimeout = 3600;

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
