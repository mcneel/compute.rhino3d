using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace compute.geometry.tests
{
    [SetUpFixture]
    public class SetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            RhinoInside.Resolver.Initialize();
            Server.SetUp();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Server.Dispose();
        }
    }

    public static class Server
    {
        static IDisposable _rhcore;
        static TestServer _server;
        public static HttpClient Client { get; private set; }

        public static void SetUp()
        {
            if (_server != null)
                return;

            _rhcore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            _server = TestServer.Create<Startup>();

            Client = _server.HttpClient;

            // use this if we update to nancy 2.0.0
            //Client = new CustomHttpClient(_server.Handler) { BaseAddress = _server.BaseAddress };
        }

        public static void Dispose()
        {
            _server?.Dispose();
            _rhcore?.Dispose();
        }
    }


    // custom http client to workaround bug in nancy 2.0.0 where request body shows up empty
    //class CustomHttpClient : HttpClient
    //{
    //    public CustomHttpClient(HttpMessageHandler handler) : base(handler)
    //    {
    //    }

    //    public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //    {
    //        // this seems to do the job...
    //        _ = request?.Content?.ReadAsStringAsync();

    //        return base.SendAsync(request, cancellationToken);
    //    }
    //}
}
