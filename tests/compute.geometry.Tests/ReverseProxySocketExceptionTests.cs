using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using rhino.compute;

namespace compute.geometry.Tests;

/// <summary>
/// Server-side unit tests that verify how ReverseProxyModule handles a
/// SocketException thrown when the HttpClient cannot reach a compute.geometry child.
///
/// These tests use WebApplicationFactory to host rhino.compute in-process and
/// inject a fake HttpMessageHandler that throws SocketException, so no real
/// server or child process is required.
///
/// On the BROKEN branch: the unhandled SocketException propagates through
/// ReverseProxyGet and ASP.NET returns 500. The test will FAIL.
///
/// On the FIXED branch: the proxy catches the SocketException and returns
/// 502 Bad Gateway. The test will PASS.
/// </summary>
[TestFixture]
public class ReverseProxySocketExceptionTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Point ComputeChildren at a fake base URL so GetComputeServerBaseUrl()
        // returns immediately without launching any real child processes.
        ComputeChildren.TestBaseUrl = "http://localhost:19999";
        ComputeChildren.SpawnOnStartup = false;

        // Inject an HttpMessageHandler that always throws SocketException
        // (connection refused) to simulate an unreachable compute.geometry child.
        var sockEx = new SocketException((int)SocketError.ConnectionRefused);
        var inner = new HttpRequestException("Connection refused", sockEx);
        var handler = new ThrowingHandler(inner);

        _factory = new WebApplicationFactory<Program>();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:19999") };
        ReverseProxyModule.OverrideHttpClient(httpClient);

        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        ComputeChildren.TestBaseUrl = null;
    }

    [Test]
    public async Task ProxyGet_WhenChildConnectionRefused_DoesNotReturn500()
    {
        var response = await _client.GetAsync("/sdk");

        Assert.That((int)response.StatusCode, Is.Not.EqualTo(500),
            "GET /sdk returned 500 — the SocketException from the child proxy is unhandled. " +
            "The fix should return 502 Bad Gateway instead.");
    }

    [Test]
    public async Task ProxyGet_WhenChildConnectionRefused_Returns502()
    {
        var response = await _client.GetAsync("/sdk");

        Assert.That((int)response.StatusCode, Is.EqualTo(502),
            $"Expected 502 Bad Gateway when compute.geometry child is unreachable, " +
            $"got {(int)response.StatusCode}.");
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _exception;
        public ThrowingHandler(Exception exception) => _exception = exception;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw _exception;
    }
}
