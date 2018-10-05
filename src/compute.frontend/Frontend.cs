using System;
using System.Text;
using System.Collections.Generic;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Gzip;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using Serilog;
using Topshelf;
using compute.frontend.Authentication;

namespace compute.frontend
{
    class Frontend
    {
        static void Main(string[] args)
        {
            Logging.Init();
            // You may need to configure the Windows Namespace reservation to assign
            // rights to use the port that you set below.
            // See: https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy
            // Use cmd.exe or PowerShell in Administrator mode with the following command:
            // netsh http add urlacl url=http://+:80/ user=Everyone
            // netsh http add urlacl url=https://+:443/ user=Everyone
            int https_port = Env.GetEnvironmentInt("COMPUTE_HTTPS_PORT", 0);
#if DEBUG
            int http_port = Env.GetEnvironmentInt("COMPUTE_HTTP_PORT", 8888);
#else
            int http_port = Env.GetEnvironmentInt("COMPUTE_HTTP_PORT", 80);
#endif

            Topshelf.HostFactory.Run(x =>
            {
                x.UseSerilog();
                x.ApplyCommandLine();
                x.SetStartTimeout(new TimeSpan(0, 1, 0));
                x.Service<NancySelfHost>(s =>
                  {
                      s.ConstructUsing(name => new NancySelfHost());
                      s.WhenStarted(tc => tc.Start(http_port, https_port));
                      s.WhenStopped(tc => tc.Stop());
                  });
                x.RunAsPrompt();
                //x.RunAsLocalService();
                x.SetDisplayName("compute.frontend");
                x.SetServiceName("compute.frontend");
            });
        }
    }

    public class NancySelfHost
    {
        private NancyHost _nancyHost;
        private System.Diagnostics.Process _backendProcess = null;
        public static bool RunningHttps { get; set; }

        public void Start(int http_port, int https_port)
        {
            var config = new HostConfiguration();
#if DEBUG
            config.RewriteLocalhost = false;  // Don't require URL registration for localhost when debugging
            if (Env.GetEnvironmentBool("COMPUTE_SPAWN_GEOMETRY_SERVER", false))  // False by default in debug so we can run both services in the debugger
                SpawnBackendProcess();
#else
            if (Env.GetEnvironmentBool("COMPUTE_SPAWN_GEOMETRY_SERVER", true))
                SpawnBackendProcess();
#endif
            var listenUriList = new List<Uri>();

            if (http_port > 0)
                listenUriList.Add(new Uri($"http://localhost:{http_port}"));
            if (https_port > 0)
                listenUriList.Add(new Uri($"https://localhost:{https_port}"));

            if (listenUriList.Count > 0)
                _nancyHost = new NancyHost(config, listenUriList.ToArray());
            else
                Log.Error("Neither COMPUTE_HTTP_PORT nor COMPIUTE_HTTPS_PORT are set. Not listening!");
            try
            {
                _nancyHost.Start();
                foreach (var uri in listenUriList)
                    Log.Information("compute.frontend running on {Uri}", uri.OriginalString);
            }
            catch (AutomaticUrlReservationCreationFailureException)
            {
                Log.Error(GetAutomaticUrlReservationCreationFailureExceptionMessage(listenUriList));
                Environment.Exit(1);
            }
        }

        // TODO: move this somewhere else
        string GetAutomaticUrlReservationCreationFailureExceptionMessage(List<Uri> listenUriList)
        {
            var msg = new StringBuilder();
            msg.AppendLine("Url not reserved. From an elevated command promt, run:");
            msg.AppendLine();
            foreach (var uri in listenUriList)
                msg.AppendLine($"netsh http add urlacl url=\"{uri.Scheme}://+:{uri.Port}/\" user=\"Everyone\"");
            return msg.ToString();
        }

        private void SpawnBackendProcess()
        {
            // Set up compute to run on a secondary process
            // Proxy requests through to the backend process.
            string backendPort = Env.GetEnvironmentString("COMPUTE_BACKEND_PORT", "8081");
            var info = new System.Diagnostics.ProcessStartInfo();
            info.UseShellExecute = false;
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                info.Environment.Add((string)entry.Key, (string)entry.Value);
            }

            info.FileName = "compute.geometry.exe";

            Log.Information("Starting back-end geometry service on port {Port}", backendPort);
            _backendProcess = System.Diagnostics.Process.Start(info);
            _backendProcess.EnableRaisingEvents = true;
            _backendProcess.Exited += _backendProcess_Exited;
        }

        private void _backendProcess_Exited(object sender, EventArgs e)
        {
            var process = sender as System.Diagnostics.Process;
            if (process?.ExitCode == -1)
                return;  // Process is closing from Ctrl+C on console

            _backendProcess = null;
            SpawnBackendProcess();
        }

        public void Stop()
        {
            if (_backendProcess != null)
                _backendProcess.Kill();
            _nancyHost.Stop();
        }
    }

    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private byte[] _favicon;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Log.Debug("ApplicationStartup");

            pipelines.EnableGzipCompression(new GzipCompressionSettings() { MinimumBytes = 1024 });

            var auth_method = Env.GetEnvironmentString("COMPUTE_AUTH_METHOD", "");

            if (auth_method == "RHINO_ACCOUNT")
                pipelines.AddAuthRhinoAccount();
            else if (auth_method == "API_KEY")
                pipelines.AddAuthApiKey();
            pipelines.AddHeadersAndLogging();
            pipelines.AddRequestStashing();

            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("docs"));
        }

        protected override byte[] FavIcon
        {
            get { return _favicon ?? (_favicon = LoadFavIcon()); }
        }

        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("compute.frontend.favicon.ico"))
            {
                var memoryStream = new System.IO.MemoryStream();
                resourceStream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }
    }
}
