using System.Threading.Tasks;
using NUnit.Framework;
using System.Net;
using Rhino.Geometry;
using System.Net.Http;
using Newtonsoft.Json;

namespace compute.geometry.tests
{
    [TestFixture]
    public class PostTests
    {
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
            var res = await Server.Client.PostAsync("rhino/geometry/mesh/createfrombrep", content);

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

    
}
