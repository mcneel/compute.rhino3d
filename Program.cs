using System;
using Nancy.Hosting.Self;
using Nancy.Extensions;
using Topshelf;
using Nancy.Conventions;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.Gzip;
using System.Collections.Generic;

namespace RhinoCommon.Rest
{
    class Program
    {
        static void Main(string[] args)
        {
            // You may need to configure the Windows Namespace reservation to assign
            // rights to use the port that you set below.
            // See: https://github.com/NancyFx/Nancy/wiki/Self-Hosting-Nancy
            // Use cmd.exe or PowerShell in Administrator mode with the following command:
            // netsh http add urlacl url=http://+:80/ user=Everyone
            // netsh http add urlacl url=https://+:443/ user=Everyone

            int https_port = 0;
#if DEBUG
            int http_port = 8888;
#else
            int http_port = 80;
#endif
            string sHttpPort = Environment.GetEnvironmentVariable("com.rhino3d.compute.HTTP_PORT");

            if (!string.IsNullOrWhiteSpace(sHttpPort))
            {
                if (!int.TryParse(sHttpPort, out http_port))
                {
                    Console.WriteLine(string.Format("environment variable com.rhino3d.compute.HTTP_PORT set to '{0}'; unable to parse as integer.", sHttpPort));
                }
            }

            string sHttpsPort = Environment.GetEnvironmentVariable("com.rhino3d.compute.HTTPS_PORT");
            if (!string.IsNullOrWhiteSpace(sHttpsPort))
            {
                if (!int.TryParse(sHttpsPort, out https_port))
                {
                    Console.WriteLine(string.Format("environment variable com.rhino3d.compute.HTTP_PORT set to '{0}'; unable to parse as integer.", sHttpsPort));
                }
            }
            Topshelf.HostFactory.Run(x =>
            {
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
                x.SetDisplayName("RhinoCommon Geometry Server");
                x.SetServiceName("RhinoCommon Geometry Server");
            });
            RhinoLib.ExitInProcess();
        }
    }

    public class NancySelfHost
    {
        private NancyHost _nancyHost;
        public static bool RunningHttps { get; set; }

        public void Start(int http_port, int https_port)
        {
            Console.WriteLine($"Launching RhinoCore library as {Environment.UserName}");
            RhinoLib.LaunchInProcess(RhinoLib.LoadMode.Headless, 0);
            var config = new HostConfiguration();
            var listenUriList = new List<Uri>();

            if (http_port > 0)
                listenUriList.Add(new Uri($"http://localhost:{http_port}"));
            if (https_port > 0)
                listenUriList.Add(new Uri($"https://localhost:{https_port}"));

            if (listenUriList.Count > 0)
                _nancyHost = new NancyHost(config, listenUriList.ToArray());
            else
                Console.WriteLine("ERROR: neither http_port nor https_port are set; NOT LISTENING!");
            try
            {
                _nancyHost.Start();
                foreach (var uri in listenUriList)
                    Console.WriteLine($"Running on {uri}");
            }
            catch (Nancy.Hosting.Self.AutomaticUrlReservationCreationFailureException)
            {
                Console.WriteLine("\r\nERROR: URL Not Reserved:\r\nFrom an elevated command promt, run:\r\n");
                foreach (var uri in listenUriList)
                    Console.WriteLine($"netsh http add urlacl url=\"{uri}\" user=\"Everyone\"\r\n");
            }
        }

        public void Stop()
        {
            _nancyHost.Stop();
        }
    }

    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private byte[] favicon;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            // Enable Compression with Settings
            var settings = new GzipCompressionSettings();
            settings.MinimumBytes = 1024;
            pipelines.EnableGzipCompression(settings);
            pipelines.AddRequestId();
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("docs"));
        }

        protected override byte[] FavIcon
        {
            get { return this.favicon ?? (this.favicon = LoadFavIcon()); }
        }

        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("RhinoCommon.Rest.favicon.ico"))
            {
                var memoryStream = new System.IO.MemoryStream();
                resourceStream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }
    }

    public class RhinoModule : Nancy.NancyModule
    {
        string GetApiToken()
        {
            var requestId = new System.Collections.Generic.List<string>(Request.Headers["api_token"]);
            if (requestId.Count != 1)
                return null;
            return requestId[0];
        }

        Nancy.HttpStatusCode CheckAuthorization()
        {
            string token = GetApiToken();
            if (string.IsNullOrWhiteSpace(token))
                return Nancy.HttpStatusCode.Unauthorized;
            if (token.Length > 2 && token.Contains("@"))
                return Nancy.HttpStatusCode.OK;
            return Nancy.HttpStatusCode.Unauthorized;
        }

        public RhinoModule()
        {
            Get["/healthcheck"] = _ => "healthy";
            Get["/version"] = _ => FixedEndpoints.GetVersion();

            var endpoints = EndPointDictionary.GetDictionary();
            foreach (var kv in endpoints)
            {
                Get[kv.Key] = _ =>
                {
                    if (NancySelfHost.RunningHttps && !Request.Url.IsSecure)
                    {
                        string url = Request.Url.ToString().Replace("http", "https");
                        return new Nancy.Responses.RedirectResponse(url, Nancy.Responses.RedirectResponse.RedirectType.Permanent);
                    }
                    Logger.WriteInfo($"GET {kv.Key}", null);
                    var response = kv.Value.HandleGetAsResponse();
                    if (response != null)
                        return response;
                    return kv.Value.HandleGet();
                };
                Post[kv.Key] = _ =>
                {
                    if (NancySelfHost.RunningHttps && !Request.Url.IsSecure)
                        return Nancy.HttpStatusCode.HttpVersionNotSupported;

                    Logger.WriteInfo($"POST {kv.Key}", GetApiToken());
                    if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Key.Length > 1)
                    {
                        var authCheck = CheckAuthorization();
                        if (authCheck != Nancy.HttpStatusCode.OK)
                            return authCheck;
                    }
                    var jsonString = Request.Body.AsString();

                    // In order to enable CORS, we add the proper headers to the response
                    var resp = new Nancy.Response();
                    resp.Contents = (e) =>
                    {
                        using (var sw = new System.IO.StreamWriter(e))
                        {
                            bool multiple = false;
                            System.Collections.Generic.Dictionary<string, string> returnModifiers = null;
                            foreach(string name in Request.Query)
                            {
                                if( name.StartsWith("return.", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (returnModifiers == null)
                                        returnModifiers = new System.Collections.Generic.Dictionary<string, string>();
                                    string dataType = "Rhino.Geometry." + name.Substring("return.".Length);
                                    string items = Request.Query[name];
                                    returnModifiers[dataType] = items;
                                    continue;
                                }
                                if (name.Equals("multiple", StringComparison.InvariantCultureIgnoreCase))
                                    multiple = Request.Query[name];
                            }
                            var postResult = kv.Value.HandlePost(jsonString, multiple, returnModifiers);
                            sw.Write(postResult);
                            sw.Flush();
                        }
                    };
                    return resp;
                };
            }
        }
    }
}
