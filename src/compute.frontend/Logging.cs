using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;

namespace compute.frontend
{
    static class Logging
    {
        static bool _enabled = false;
        public static void Init()
        {
            if (_enabled)
                return;

            var path = Path.Combine(Path.GetTempPath(), "Compute", "Logs", "log-frontend-.txt"); // log-20180925.txt, etc.
            var limit = Env.GetEnvironmentInt("COMPUTE_LOG_RETAIN_DAYS", 10);

            var logger = new LoggerConfiguration()
//#if DEBUG
                .MinimumLevel.Debug()
//#endif
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Source", "frontend")
                .WriteTo.Console(outputTemplate: "{Timestamp:o} {Level:w3}: {Source} {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(new JsonFormatter(), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            var cloudwatch_enabled = false;
            var aws_log_group = Env.GetEnvironmentString("COMPUTE_LOG_CLOUDWATCH_GROUP", "/compute/dev");

            if (Env.GetEnvironmentBool("COMPUTE_LOG_CLOUDWATCH", true))
            {
                var options = new CloudWatchSinkOptions
                {
                    LogGroupName = aws_log_group,
                    MinimumLogEventLevel = LogEventLevel.Debug,
                    TextFormatter = new JsonFormatter(),
                    LogGroupRetentionPolicy = LogGroupRetentionPolicy.SixMonths,
#if !DEBUG
                    Period = TimeSpan.FromSeconds(60)
#endif
                };

                var aws_region_string = Env.GetEnvironmentString("AWS_REGION_ENDPOINT", "us-east-1");
                var aws_region_endpoint = Amazon.RegionEndpoint.GetBySystemName(aws_region_string);
                var aws_client = new AmazonCloudWatchLogsClient(aws_region_endpoint);

                logger.WriteTo.AmazonCloudWatch(options, aws_client);

                cloudwatch_enabled = true;
            }

            Log.Logger = logger.CreateLogger();

            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            if (cloudwatch_enabled)
                Log.ForContext("LogGroup", aws_log_group)
                    .Debug("Amazon CloudWatch logging enabled");

            _enabled = true;
        }
    }
}
