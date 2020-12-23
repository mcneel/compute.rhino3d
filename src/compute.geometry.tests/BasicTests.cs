using System;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;

namespace compute.geometry.tests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public async Task HelloWorld()
        {
            // Server.Client is a custom HttpClient that is configured to communicate with the TestServer
            var res = await Server.Client.GetAsync("");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.SeeOther));
        }

        [Test]
        public async Task CanListSdk()
        {
            var res = await Server.Client.GetAsync("sdk");

            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(res.Content.Headers.ContentType.MediaType, Is.EqualTo("text/html"));

            // see PythonRouteExists()
            //var body = await res.Content.ReadAsStringAsync();
            //Assert.That(body, Contains.Substring("python"));
        }

        [Test]
        public async Task RouteExists()
        {
            // this kind of test could be used to check that a particular endpoint exists
            var res = await Server.Client.GetAsync("rhino/geometry/curve/getlength");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(res.Content.Headers.ContentType.MediaType, Is.EqualTo("text/html"));
        }

        // TODO: figure out why python route (configured in a compute plug-in) doesn't exist in test server
        //[Test]
        //public async Task PythonRouteExists()
        //{
        //    var res = await Server.Client.GetAsync("rhino/python/evaluate");

        //    Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        //    Assert.That(res.Content.Headers.ContentType.MediaType, Is.EqualTo("text/html"));
        //}
    }

    
}
