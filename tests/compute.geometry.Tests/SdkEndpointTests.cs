using System.Net.Sockets;
using NUnit.Framework;

namespace compute.geometry.Tests;

/// <summary>
/// Integration tests for the compute.geometry /sdk endpoints.
/// These tests require a running compute.geometry server on the configured base URL.
/// A SocketException (connection refused) indicates the server is not reachable,
/// which surfaces the connection-refused bug being fixed in a separate branch.
/// </summary>
[TestFixture]
public class SdkEndpointTests
{
    // Override via RHINO_COMPUTE_URLS environment variable, e.g. "http://localhost:8082"
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("RHINO_COMPUTE_URLS")?.Split(';')[0].TrimEnd('/')
        ?? "http://localhost:6500";

    private static HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
    }

    private static bool IsConnectionRefused(HttpRequestException ex)
    {
        var inner = ex.InnerException;
        while (inner != null)
        {
            if (inner is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
                return true;
            inner = inner.InnerException;
        }
        return false;
    }

    [Test]
    public async Task GetSdk_ReturnsSuccessStatusCode()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk");
        }
        catch (HttpRequestException ex) when (IsConnectionRefused(ex))
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"Ensure compute.geometry is running. Inner: {ex.InnerException?.Message}");
            return;
        }

        Assert.That((int)response.StatusCode, Is.InRange(200, 299),
            $"GET /sdk returned unexpected status {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    [Test]
    public async Task GetSdk_ResponseContainsHtmlBody()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk");
        }
        catch (HttpRequestException ex) when (IsConnectionRefused(ex))
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"Ensure compute.geometry is running. Inner: {ex.InnerException?.Message}");
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("<!DOCTYPE html>").Or.Contain("<html"),
            "GET /sdk response body does not appear to be HTML");
    }

    /// <summary>
    /// Asserts that the proxy layer never returns a raw 500 when a compute.geometry
    /// child is unreachable. A 500 indicates an unhandled SocketException in the
    /// reverse proxy — the fix should return a proper 502 Bad Gateway instead.
    ///
    /// To reliably trigger the failure on the broken branch, run this test while
    /// compute.geometry children are unavailable (e.g. kill child processes, or run
    /// immediately after rhino.compute starts before children are ready).
    /// </summary>
    [Test]
    public async Task GetSdk_DoesNotReturnInternalServerError()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk");
        }
        catch (HttpRequestException ex) when (IsConnectionRefused(ex))
        {
            Assert.Ignore($"rhino.compute is not reachable at {BaseUrl} — cannot run proxy error-handling test.");
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.That((int)response.StatusCode, Is.Not.EqualTo(500),
            $"GET /sdk returned 500 Internal Server Error — this indicates an unhandled SocketException " +
            $"in the reverse proxy when a compute.geometry child is unreachable. Body: {body}");
    }

}
