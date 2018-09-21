using System;
using System.Collections.Generic;
using Nancy;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace RhinoCommon.Rest
{
    public enum LogLevels
    {
        Debug,
        Info,
        Warning,
        Error
    }
    public interface ILogger
    {
        void Log(LogLevels severity, JObject log);
    }

    public class TempFileLogger : ILogger
    {
        private string m_logfile = null;
        private StreamWriter m_writer = null;
        private readonly object m_writeLock = new object();
        private DateTime m_logStartDay = DateTime.UtcNow;
        private int m_daysToKeep = Env.GetEnvironmentInt("COMPUTE_LOG_RETAIN_DAYS", 10);

        static string LogFolder
        {
            get
            {
                var logFolder = Path.Combine(Path.GetTempPath(), "Compute", "Logs");
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                return logFolder;
            }
        }

        private void Setup()
        {
            if (m_logStartDay.Day != DateTime.UtcNow.Day && m_writer != null)
            {
                // Cause a new log file to be written every day.
                m_writer.Close();
                m_writer = null;
                m_logfile = null;

                // Delete old logs
                foreach (var file in Directory.GetFiles(LogFolder))
                {
                    var created = File.GetCreationTime(file);
                    var threshold = DateTime.Now.AddDays(-m_daysToKeep);
                    if (created < threshold)
                        File.Delete(file);
                }
            }

            if (m_writer != null)
                return;

            if (string.IsNullOrWhiteSpace(m_logfile))
            {
                var now = DateTime.UtcNow.ToString("yyyy-MM-dd HHmmss");
                m_logfile = Path.Combine(LogFolder, string.Format("{0}.log", now));
                m_logStartDay = DateTime.UtcNow;
            }

            m_writer = new StreamWriter(File.Open(m_logfile, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        }

        public void Log(LogLevels severity, JObject log)
        {
            lock (m_writeLock)
            {
                Setup();
                m_writer.WriteLine(log.ToString(Newtonsoft.Json.Formatting.None));
                m_writer.Flush();
            }
        }
    }


    class Logger
    {
        private static ILogger m_logger = null;
        public static void Init(ILogger sink)
        {
            m_logger = sink;
            Logger.Info(null, "Logging enabled using {0}", sink.GetType().Name);
        }

        public static void Info(NancyContext context, string format, params object[] args)
        {
            Write(context, LogLevels.Info, format, args);
        }

        public static void Info(NancyContext context, Dictionary<string, string> data)
        {
            Write(context, LogLevels.Info, data);
        }

        public static void Debug(NancyContext context, string format, params object[] args)
        {
            Write(context, LogLevels.Debug, format, args);
        }
        public static void Debug(NancyContext context, Dictionary<string, string> data)
        {
            Write(context, LogLevels.Debug, data);
        }

        public static void Warning(NancyContext context, string format, params object[] args)
        {
            Write(context, LogLevels.Warning, format, args);
        }
        public static void Warning(NancyContext context, Dictionary<string, string> data)
        {
            Write(context, LogLevels.Warning, data);
        }

        public static void Error(NancyContext context, string format, params object[] args)
        {
            Write(context, LogLevels.Error, format, args);
        }
        public static void Error(NancyContext context, Dictionary<string, string> data)
        {
            Write(context, LogLevels.Error, data);
        }

        static void Write(NancyContext context, LogLevels severity, string format, params object[] args)
        {
            if (m_logger == null)
                return;

            var message = string.Format(format, args);
            Console.WriteLine(string.Format("{0}: {1}", severity.ToString(), message));
            var data = new Dictionary<string, string>();
            data.Add("message", message);
            Write(context, severity, data);
        }

        static void Write(NancyContext context, LogLevels severity, Dictionary<string, string> data)
        {
            if (m_logger == null)
                return;

            var log = new JObject();
            log.Add("dateTime", DateTime.UtcNow.ToString("o")); // ISO 8601 format
            log.Add("severity", severity.ToString());

            foreach(var pair in data)
            {
                log.Add(pair.Key, pair.Value);
            }

            if (context != null)
            {
                object item = null;
                if (context.Request != null)
                {
                    log.Add("sourceIpAddress", context.Request.UserHostAddress);
                    log.Add("path", context.Request.Url.Path);
                    log.Add("query", context.Request.Url.Query);
                    log.Add("method", context.Request.Method);
                }

                if (context.Items != null)
                {
                    if (context.Items.TryGetValue("x-compute-id", out item))
                        log.Add("requestId", item as string);
                    if (context.Items.TryGetValue("x-compute-host", out item))
                        log.Add("computeHost", item as string);
                    if (context.Items.TryGetValue("auth_user", out item))
                        log.Add("auth_user", item as string);
                }
            }

            m_logger.Log(severity, log);
        }
    }
}
