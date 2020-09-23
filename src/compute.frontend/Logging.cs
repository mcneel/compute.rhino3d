using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace compute.frontend
{
    static class Logging
    {
        static bool _enabled = false;
        public static void Init()
        {
            if (_enabled)
                return;

            var dir = Env.GetEnvironmentString("COMPUTE_LOG_PATH", Path.Combine(Path.GetTempPath(), "Compute", "Logs"));
            var path = Path.Combine(dir, "log-frontend-.txt"); // log-20180925.txt, etc.
            var limit = Env.GetEnvironmentInt("COMPUTE_LOG_RETAIN_DAYS", 10);

            var logger = new LoggerConfiguration()
//#if DEBUG
                .MinimumLevel.Debug()
//#endif
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Source", "frontend")
                .WriteTo.Console(outputTemplate: "{Timestamp:o} {Level:w3}: {Source} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            Log.Logger = logger.CreateLogger();
            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            _enabled = true;
        }
    }
}
