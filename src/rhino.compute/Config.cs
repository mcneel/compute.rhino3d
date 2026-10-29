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
        const string RHINO_COMPUTE_COMPRESSION = "RHINO_COMPUTE_COMPRESSION";
        // Shared with the RhinoAI plug-in (MCPSpawnCommand.PortEnvVar): it is the variable the
        // plug-in reads to start its listener, so one setting opts the whole stack in.
        const string RHINO_MCP_AUTOSTART_PORT = "RHINO_MCP_AUTOSTART_PORT";
        const string RHINO_COMPUTE_MCP_SESSION_TIMEOUT = "RHINO_COMPUTE_MCP_SESSION_TIMEOUT";
        const string RHINO_COMPUTE_MCP_MAX_SESSIONS = "RHINO_COMPUTE_MCP_MAX_SESSIONS";

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
