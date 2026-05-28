using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;

namespace compute.geometry
{
    // Reads a mutable static port at log-event time so the prefix can switch from "CG " to
    // "CG NNNN" mid-run — useful for standalone launches where the port isn't known until
    // Kestrel binds (default 5000). When port == 0, the property is not added and the
    // output template's {Port} placeholder renders empty.
    sealed class DynamicPortEnricher : ILogEventEnricher
    {
        static int port;
        public static void SetPort(int port) => DynamicPortEnricher.port = port;
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            int port = DynamicPortEnricher.port;
            if (port > 0)
                logEvent.AddPropertyIfAbsent(factory.CreateProperty("Port", port));
        }
    }

    static class Logging
    {
        static bool enabled = false;
        public static List<string> Warnings { get; set; }
        public static List<string> Errors { get; set; }

        /// <summary>
        /// Initialises globally-shared logger.
        /// </summary>
        /// <param name="port">Port this child is listening on if already known at init time
        /// (i.e. passed via -port:N from rhino.compute). Pass 0 when the port is unknown — the
        /// prefix renders without a port, and a later call to <see cref="DynamicPortEnricher.SetPort"/>
        /// (typically from the Kestrel ApplicationStarted callback) will start populating it.</param>
        public static void Init(int port = 0)
        {
            if (enabled)
                return;
            if (Warnings == null)
                Warnings = new List<string>();
            if (Errors == null)
                Errors = new List<string>();

            var path = Path.Combine(Config.LogPath, "log-compute-geometry-.txt"); // log-geometry-20180925.txt, etc.
            var limit = Config.LogRetainDays;
            var level = Config.Debug ? LogEventLevel.Debug : LogEventLevel.Information;

            // Seed the dynamic enricher when port is already known; otherwise leave it at 0 so the
            // {Port} placeholder renders empty until something calls DynamicPortEnricher.SetPort.
            if (port > 0)
                DynamicPortEnricher.SetPort(port);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                // Silence ASP.NET Core's built-in "An unhandled exception has occurred while
                // executing the request." log emitted by ExceptionHandlerMiddleware. Our own
                // app.UseExceptionHandler handler logs a categorized, formatted version with
                // the same stack trace, so without this override the same exception shows up
                // twice on the console.
                .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Fatal)
                .Enrich.With(new DynamicPortEnricher())
                // ANSI theme embeds colors as escape sequences in the output text so they
                // survive rhino.compute's stdout pipe (the default SystemConsoleTheme.Literate
                // uses Console.ForegroundColor, a Win32 API that has no effect on a piped handle).
                // applyThemeToRedirectedOutput: true is required because the sink otherwise
                // swaps our theme for ConsoleTheme.None whenever Console.IsOutputRedirected.
                .WriteTo.Console(
                    outputTemplate: "CG {Port} [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate,
                    applyThemeToRedirectedOutput: true)
                .WriteTo.File(new ExpressionTemplate("CG {Port} [{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}"), path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: limit);

            Log.Logger = logger.CreateLogger();

            // log warnings if deprecated env vars used
            foreach (var msg in Config.GetDeprecationWarnings())
                Log.Warning(msg);

            Log.Debug("Logging to {LogPath}", Path.GetDirectoryName(path));

            enabled = true;
        }

        internal static void LogExceptionData(System.Exception ex)
        {
            if (Errors != null)
                Errors.Add(ex.Message);
            //if (!Config.Debug)
            //    return;
            if (ex?.Data != null)
            {
                // TODO: skip useless keys once we figure out what those are
                foreach (var key in ex.Data.Keys)
                {
                    Log.Debug($"{key} : {{Data}}", ex.Data[key]);
                }
            }
        }
    }
}
