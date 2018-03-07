using System;
using Nancy.Hosting.Self;
using Nancy.Extensions;
using Topshelf;

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
#if DEBUG
            int port = 80;
            bool https = false;
#else
            int port = 443;
            bool https = true;
#endif
            Topshelf.HostFactory.Run(x =>
            {
                x.AddCommandLineDefinition("port", p => port = int.Parse(p));
                x.AddCommandLineDefinition("https", b => https = bool.Parse(b));
                x.ApplyCommandLine();
                x.SetStartTimeout(new TimeSpan(0, 1, 0));
                x.Service<NancySelfHost>(s =>
          {
                  s.ConstructUsing(name => new NancySelfHost());
                  s.WhenStarted(tc => tc.Start(https, port));
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

        public void Start(bool https, int port)
        {
            Console.WriteLine($"Launching RhinoCore library as {Environment.UserName}");
            RhinoLib.LaunchInProcess(0, 0);
            var config = new HostConfiguration();
            string address = $"http://localhost:{port}";
            if( https )
            {
                RunningHttps = true;
                address = $"https://localhost:{port}";
                _nancyHost = new NancyHost(config, new Uri(address), new Uri("http://localhost:80"));
            }
            else
            {
                _nancyHost = new NancyHost(config, new Uri(address));
            }
            _nancyHost.Start();
            Console.WriteLine("Running on " + address);
        }

        public void Stop()
        {
            _nancyHost.Stop();
        }
    }

    public class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private byte[] favicon;

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

            var endpoints = EndPointDictionary.GetDictionary();
            foreach (var kv in endpoints)
            {
                Get[kv.Key] = _ =>
                {
                    if( NancySelfHost.RunningHttps && !Request.Url.IsSecure )
                    {
                        string url = Request.Url.ToString().Replace("http","https");
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
                    resp.Headers.Add("Access-Control-Allow-Origin", "*");
                    resp.Headers.Add("Access-Control-Allow-Methods", "POST,GET");
                    resp.Headers.Add("Access-Control-Allow-Headers", "Accept, Origin, Content-type");
                    resp.Contents = (e) =>
            {
                      using (var sw = new System.IO.StreamWriter(e))
                      {
                          sw.Write(kv.Value.HandlePost(jsonString));
                          sw.Flush();
                      }
                  };
                    return resp;
                };
            }
        }
    }
}
