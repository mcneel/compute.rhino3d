using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Owin.Testing;
using System.Net;
using Rhino.Geometry;
using System.Net.Http;
using Newtonsoft.Json;

namespace compute.geometry.tests
{
    [TestFixture]
    public class Tests
    {
        IDisposable _rhcore;
        TestServer _server;

        [OneTimeSetUp]
        public void SetUp()
        {
            // RhinoCore is instatiated outside of Nancy's Bootstrapper, so we need to do it here
            // (initial experiemnts with moving it into the bootstrapper caused intermittent errors in the tests)
            // calling this more than once, even with using/dispose, causes catastrophic failure
            // TODO: set up globally, so we can split tests into more classes
            _rhcore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);

            _server = TestServer.Create<Startup>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _rhcore?.Dispose();
            _server?.Dispose();
        }

        [Test]
        public async Task HelloWorld()
        {
            var res = await _server.HttpClient.GetAsync("");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.SeeOther));
        }

        [Test]
        public async Task RouteExists()
        {
            // this kind of test could be used to check that a particular endpoint exists
            var res = await _server.HttpClient.GetAsync("rhino/geometry/curve/getlength");
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task CanMeshSphereBrep()
        {
            // create a brep sphere
            var sphere = new Sphere(Point3d.Origin, 12).ToBrep();

            // construct request (without the client since we're not testing that here)
            var options = new Rhino.FileIO.SerializationOptions();
            var sphereJson = "[" + sphere.ToJSON(options) + "]";
            var content = new StringContent(sphereJson, System.Text.Encoding.UTF8, "application/json");

            // make request
            var res = await _server.HttpClient.PostAsync("rhino/geometry/mesh/createfrombrep", content);

            // check response
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(res.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

            // read response body
            var body = await res.Content.ReadAsStringAsync();
            Assert.That(body, Is.Not.Empty);

            // deserialise and check mesh
            var meshes = JsonConvert.DeserializeObject<Mesh[]>(body);
            Assert.That(meshes, Has.Length.EqualTo(1));
            Assert.That(meshes[0].Vertices, Has.Count.EqualTo(561));
        }
    }

    [SetUpFixture]
    public class SetUp
    {
        // IDisposable _rhcore;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            RhinoInside.Resolver.Initialize();
            RhinoInside.Resolver.RhinoSystemDirectory = @"C:\Program Files\Rhino 7\System";

            // TOOD: figure out why this doesn't work...
            //_rhcore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
        }

        //[OneTimeTearDown]
        //public void OneTimeTearDown()
        //{
        //    _rhcore?.Dispose();
        //}
    }
}
