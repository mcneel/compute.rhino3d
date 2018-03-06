using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Logging.V2;

namespace RhinoCommon.Rest
{
    class Logger
    {
        const string _projectId = "compute-rhino3d";
        static LoggingServiceV2Client _client;
        static LoggingServiceV2Client Client
        {
            get
            {
                if (_client == null)
                    _client = LoggingServiceV2Client.Create();
                return _client;
            }
        }

        static bool _enabled = true;
        static bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        static bool _initialized = false;
        public static void WriteInfo(string message, string apiToken)
        {
            if (!Enabled)
                return;
            if(!_initialized)
            {
                _initialized = true;
#if DEBUG
                Console.WriteLine("Logging disabled in debug builds");
                Enabled = false;
                return;
#else
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if( assembly!=null )
                {
                    var dir = System.IO.Path.GetDirectoryName(assembly.Location);
                    dir = System.IO.Path.Combine(dir, "deployment");
                    var jsonpath = System.IO.Directory.GetFiles(dir, "*.json");
                    System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonpath[0]);
                }
#endif
            }

            string logId = "info";
            LogName logName = new LogName(_projectId, logId);
            LogNameOneof logNameToWrite = LogNameOneof.From(logName);

            LogEntry entry = new LogEntry
            {
                LogName = logName.ToString(),
                Severity = Google.Cloud.Logging.Type.LogSeverity.Info,
                TextPayload = message
            };

            IDictionary<string, string> entryLabels = null;
            if (!string.IsNullOrWhiteSpace(apiToken))
            {
                entryLabels = new Dictionary<string, string>();
                entryLabels.Add("api_token", apiToken);
            }

            Google.Api.MonitoredResource resource = new Google.Api.MonitoredResource();
            resource.Type = "global";

            try
            {
                Client.WriteLogEntries(logNameToWrite, resource, entryLabels, new LogEntry[] { entry });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
