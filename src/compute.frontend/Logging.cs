using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var path = Path.Combine(Path.GetTempPath(), "Compute", "Logs", "log-.txt"); // log-20180925.txt, etc.
            var limit = Env.GetEnvironmentInt("COMPUTE_LOG_RETAIN_DAYS", 10);

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Source", "frontend")
                .WriteTo.Console(outputTemplate: "{Timestamp:o} {Level:w3}: {Source} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit)
                // TODO: cloudwatch, stackdriver
                .CreateLogger();

            _enabled = true;
        }
    }
}
