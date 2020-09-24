using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;

namespace compute.geometry
{
    static class Logging
    {
        static bool _enabled = false;

        public static void Init()
        {
            if (_enabled)
                return;

            var path = Path.Combine(Config.LogPath, "log-geometry-.txt"); // log-geometry-20180925.txt, etc.
            var limit = Config.LogRetainDays;

            var logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .Enrich.FromLogContext()
                //.Enrich.WithProperty("Source", "geometry")
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            Log.Logger = logger.CreateLogger();

            // log warnings if deprecated env vars used
            foreach (var msg in Config.GetDeprecationWarnings())
                Log.Warning(msg);

            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            _enabled = true;
        }
    }
}
