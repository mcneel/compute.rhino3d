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
            int port = 80;
            string secret = null;
            bool https = false;
            Topshelf.HostFactory.Run(x =>
            {
                x.AddCommandLineDefinition("port", p => port = int.Parse(p));
                x.AddCommandLineDefinition("secret", s => secret = s);
                x.AddCommandLineDefinition("https", b => https = bool.Parse(b));
                x.ApplyCommandLine();
                x.SetStartTimeout(new TimeSpan(0, 1, 0));
                x.Service<NancySelfHost>(s =>
          {
                  s.ConstructUsing(name => new NancySelfHost());
                  s.WhenStarted(tc => tc.Start(https, port, secret));
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

        public void Start(bool https, int port, string secret)
        {
            RhinoModule.Secret = secret;
            Console.WriteLine($"Launching RhinoCore library as {Environment.UserName}");
            RhinoLib.LaunchInProcess(0, 0);
            var config = new HostConfiguration();
            string address = https ? $"https://localhost:{port}" :
                                     $"http://localhost:{port}";
            _nancyHost = new NancyHost(config, new Uri(address));
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
        public static string Secret { get; set; }

        Nancy.HttpStatusCode CheckSecret()
        {
            if (string.IsNullOrWhiteSpace(Secret))
                return Nancy.HttpStatusCode.OK;
            var request_secret = new System.Collections.Generic.List<string>(Request.Headers["secret"]);
            if (request_secret[0].Equals(Secret, StringComparison.Ordinal))
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
                    Logger.WriteInfo($"GET {kv.Key}");
                    var authCheck = CheckSecret();
                    if (authCheck != Nancy.HttpStatusCode.OK)
                        return authCheck;
                    return kv.Value.HandleGet();
                };
                Post[kv.Key] = _ =>
                {
                    Logger.WriteInfo($"POST {kv.Key}");
                    var authCheck = CheckSecret();
                    if (authCheck != Nancy.HttpStatusCode.OK)
                        return authCheck;
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
