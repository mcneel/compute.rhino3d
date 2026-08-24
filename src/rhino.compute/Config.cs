using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Events;

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
        /// RHINO_COMPUTE_LOG_LEVEL: the lowest level the logs record (Verbose, Debug, Information,
        /// Warning, Error or Fatal). Unlike RHINO_COMPUTE_DEBUG, it doesn't change what callers get
        /// back. Defaults to Debug when RHINO_COMPUTE_DEBUG is on, otherwise Information.
        /// </summary>
        public static LogEventLevel LogLevel { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT: seconds to wait for a newly spawned
        /// compute.geometry child to open its port before giving up on it.
        ///
        /// A child does not bind its port until Rhino, Grasshopper and the compute
        /// plug-ins have all finished loading — compute.geometry's Startup.Configure
        /// calls RhinoCoreStartup() synchronously, so the whole Rhino boot happens
        /// before Kestrel listens.
        ///
        /// On a freshly created cloud instance that load is dominated by first-touch
        /// reads of the Rhino + Grasshopper file set. Where the volume was created
        /// from a snapshot, those blocks are fetched on demand, so the very first
        /// child pays a one-time cost far above the steady-state figure.
        /// </summary>
        public static int ChildStartupTimeout { get; private set; }

        /// <summary>
        /// Default for <see cref="ChildStartupTimeout"/>. Public so callers can tell
        /// whether the running value was tuned or left alone.
        ///
        /// Measured, not guessed. On an AWS Marketplace instance (t3.xlarge, us-east-1,
        /// Rhino 8.34.26230) the first child on a virgin EBS volume took 119.2s to open
        /// its port; every later child on the same volume took 7-11s regardless of how
        /// long the instance had been up. So the cost is one-time per volume, not a
        /// warm-up curve — which means the previous 60s default did not fail
        /// intermittently, it failed on the first request to EVERY new instance.
        ///
        /// 300s is ~2.5x that worst case. The headroom covers what the measurement did
        /// not sample: smaller or burstable instance types, other regions, and
        /// user-installed Grasshopper plug-ins, all of which add load time. The only
        /// cost of the headroom is how long a genuinely broken child takes to report
        /// failure.
        ///
        /// Note this bound is necessary but not sufficient for a fast first call: 119s
        /// still exceeds many HTTP clients' own timeouts. Loading a child at startup
        /// (--spawn-on-startup) is what moves that cost off the first request.
        ///
        /// Historical note: the previous value of 60 dates from March 2021 (PR #241),
        /// chosen for children launched locally by Hops on a developer workstation.
        /// </summary>
        public const int DefaultChildStartupTimeout = 300;

        /// <summary>
        /// RHINO_COMPUTE_CHILDCOUNT: number of child compute.geometry processes to run.
        /// Settable on a deployed server without editing web.config. Clamped to
        /// [1, <see cref="ComputeChildren.MaxChildren"/>].
        /// </summary>
        public static int ChildCount { get; private set; }

        /// <summary>Default for <see cref="ChildCount"/> when neither the flag nor the env var is set.</summary>
        public const int DefaultChildCount = 4;

        /// <summary>
        /// RHINO_COMPUTE_IDLESPAN: seconds a child stays loaded between requests before it
        /// shuts down (and stops incurring the metered software charge). Settable on a
        /// deployed server without editing web.config. Clamped to [60, 86400].
        /// </summary>
        public static int IdleSpanSeconds { get; private set; }

        /// <summary>Default for <see cref="IdleSpanSeconds"/> (1 hour).</summary>
        public const int DefaultIdleSpanSeconds = 3600;

        /// <summary>
        /// Configuration problems found by <see cref="Load"/>, for logging once the logger exists.
        /// </summary>
        public static IReadOnlyList<string> Warnings => warnings;

        /// <summary>
        /// RHINO_COMPUTE_COMPRESSION: accept gzip request bodies and gzip answers for clients
        /// that ask (Accept-Encoding: gzip). On by default; false turns it off for clients that
        /// can't handle it. Children inherit the variable, so standalone and child compute.geometry agree.
        /// </summary>
        public static bool Compression { get; private set; }

        /// <summary>
        /// RHINO_MCP_AUTOSTART_PORT: turns on the /mcp route. The value is the loopback port the
        /// dedicated MCP child's RhinoAI listener is asked to bind; the plug-in reads the same
        /// variable. Unset or 0: /mcp is off and no child starts an MCP listener.
        /// </summary>
        public static int McpPort { get; private set; }

        /// <summary>True when <see cref="McpPort"/> is set, i.e. POST /mcp is served.</summary>
        public static bool McpEnabled => McpPort > 0;

        /// <summary>
        /// RHINO_COMPUTE_MCP_SESSION_TIMEOUT: seconds an MCP session (one headless document in
        /// the MCP child) may sit idle before it is closed. Default 3600.
        /// </summary>
        public static int McpSessionTimeout { get; private set; }

        /// <summary>
        /// RHINO_COMPUTE_MCP_MAX_SESSIONS: cap on concurrent MCP sessions. Default 32.
        /// </summary>
        public static int McpMaxSessions { get; private set; }

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

            // Clamp (rather than reset) so an operator who asks for more children than the
            // cap still gets the cap - preserving the previous --childcount cap-to-max
            // behaviour - and a nonsensical low value floors to 1 instead of failing.
            ChildCount = GetEnvironmentVariable(RHINO_COMPUTE_CHILDCOUNT, DefaultChildCount);
            if (ChildCount < 1 || ChildCount > ComputeChildren.MaxChildren)
            {
                int clamped = Math.Clamp(ChildCount, 1, ComputeChildren.MaxChildren);
                warnings.Add($"{RHINO_COMPUTE_CHILDCOUNT} set to '{ChildCount}'; outside the supported " +
                             $"range 1-{ComputeChildren.MaxChildren}. Using {clamped}.");
                ChildCount = clamped;
            }

            IdleSpanSeconds = GetEnvironmentVariable(RHINO_COMPUTE_IDLESPAN, DefaultIdleSpanSeconds);
            if (IdleSpanSeconds < MinIdleSpanSeconds || IdleSpanSeconds > MaxIdleSpanSeconds)
            {
                int clamped = Math.Clamp(IdleSpanSeconds, MinIdleSpanSeconds, MaxIdleSpanSeconds);
                warnings.Add($"{RHINO_COMPUTE_IDLESPAN} set to '{IdleSpanSeconds}'; outside the supported " +
                             $"range {MinIdleSpanSeconds}-{MaxIdleSpanSeconds} seconds. Using {clamped}.");
                IdleSpanSeconds = clamped;
            }

            Compression = GetEnvironmentVariable(RHINO_COMPUTE_COMPRESSION, true);

            McpPort = GetEnvironmentVariable(RHINO_MCP_AUTOSTART_PORT, 0);
            if (McpPort < 0 || McpPort > 65535)
            {
                warnings.Add($"{RHINO_MCP_AUTOSTART_PORT} set to '{McpPort}'; use a port in range 1-65535. /mcp is off.");
                McpPort = 0;
            }
            McpSessionTimeout = GetEnvironmentVariable(RHINO_COMPUTE_MCP_SESSION_TIMEOUT, 3600);
            McpMaxSessions = GetEnvironmentVariable(RHINO_COMPUTE_MCP_MAX_SESSIONS, 32);

#if DEBUG
            Debug = true;
#elif RELEASE
            Debug = false;
#endif
            Debug = GetEnvironmentVariable(RHINO_COMPUTE_DEBUG, Debug);
            LogLevel = GetLogLevel(RHINO_COMPUTE_LOG_LEVEL, Debug ? LogEventLevel.Debug : LogEventLevel.Information);
        }

        #region private
        // environment variables
        const string RHINO_COMPUTE_KEY = "RHINO_COMPUTE_KEY";
        const string RHINO_COMPUTE_TIMEOUT = "RHINO_COMPUTE_TIMEOUT";
        const string RHINO_COMPUTE_MAX_REQUEST_SIZE = "RHINO_COMPUTE_MAX_REQUEST_SIZE";
        const string RHINO_COMPUTE_LOG_PATH = "RHINO_COMPUTE_LOG_PATH";
        const string RHINO_COMPUTE_LOG_RETAIN_DAYS = "RHINO_COMPUTE_LOG_RETAIN_DAYS";
        const string RHINO_COMPUTE_DEBUG = "RHINO_COMPUTE_DEBUG";
        const string RHINO_COMPUTE_LOG_LEVEL = "RHINO_COMPUTE_LOG_LEVEL";
        const string RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT = "RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT";
        const string RHINO_COMPUTE_CHILDCOUNT = "RHINO_COMPUTE_CHILDCOUNT";
        const string RHINO_COMPUTE_IDLESPAN = "RHINO_COMPUTE_IDLESPAN";
        const string RHINO_COMPUTE_COMPRESSION = "RHINO_COMPUTE_COMPRESSION";
        // Shared with the RhinoAI plug-in (MCPSpawnCommand.PortEnvVar): it is the variable the
        // plug-in reads to start its listener, so one setting opts the whole stack in.
        const string RHINO_MCP_AUTOSTART_PORT = "RHINO_MCP_AUTOSTART_PORT";
        const string RHINO_COMPUTE_MCP_SESSION_TIMEOUT = "RHINO_COMPUTE_MCP_SESSION_TIMEOUT";
        const string RHINO_COMPUTE_MCP_MAX_SESSIONS = "RHINO_COMPUTE_MCP_MAX_SESSIONS";

        // Bounds for ChildStartupTimeout. The floor is well below any realistic Rhino
        // load time and exists only to reject 0/negative; the ceiling is an hour, past
        // which a stuck child should be diagnosed rather than waited on.
        const int MinChildStartupTimeout = 5;
        const int MaxChildStartupTimeout = 3600;

        // Bounds for IdleSpanSeconds. Floor of 60s keeps a child alive long enough to serve
        // a burst without thrashing cold starts; ceiling of 24h caps how long an idle child
        // (and its metered charge) lingers after the last request.
        const int MinIdleSpanSeconds = 60;
        const int MaxIdleSpanSeconds = 86400;

        readonly static List<string> warnings = new List<string>();

        static LogEventLevel GetLogLevel(string name, LogEventLevel defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            if (Enum.TryParse(value.Trim(), true, out LogEventLevel level) && Enum.IsDefined(typeof(LogEventLevel), level))
                return level;
            warnings.Add($"{name} set to '{value}'; use Verbose, Debug, Information, Warning, Error or Fatal. Using {defaultValue}.");
            return defaultValue;
        }

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
