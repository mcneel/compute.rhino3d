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
        ?? "http://localhost:5000";

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

    [Test]
    public async Task GetSdk_ReturnsSuccessStatusCode()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk");
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"Ensure compute.geometry is running. SocketException: {ex.InnerException.Message}");
            return;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"SocketException: {ex.Message}");
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
        catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"Ensure compute.geometry is running. SocketException: {ex.InnerException.Message}");
            return;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"SocketException: {ex.Message}");
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("<!DOCTYPE html>").Or.Contain("<html"),
            "GET /sdk response body does not appear to be HTML");
    }

    [Test]
    public async Task GetSdk_ResponseContainsCSharpSdkLink()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk");
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"Ensure compute.geometry is running. SocketException: {ex.InnerException.Message}");
            return;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk — server is not reachable. " +
                        $"SocketException: {ex.Message}");
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("/sdk/csharp"),
            "GET /sdk response does not contain a link to /sdk/csharp");
    }

    [Test]
    public async Task GetSdkCSharp_ReturnsSuccessStatusCode()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk/csharp");
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk/csharp — server is not reachable. " +
                        $"Ensure compute.geometry is running. SocketException: {ex.InnerException.Message}");
            return;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk/csharp — server is not reachable. " +
                        $"SocketException: {ex.Message}");
            return;
        }

        Assert.That((int)response.StatusCode, Is.InRange(200, 299),
            $"GET /sdk/csharp returned unexpected status {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    [Test]
    public async Task GetSdkCSharp_ResponseIsPlainText()
    {
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync("/sdk/csharp");
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk/csharp — server is not reachable. " +
                        $"Ensure compute.geometry is running. SocketException: {ex.InnerException.Message}");
            return;
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            Assert.Fail($"Connection refused to {BaseUrl}/sdk/csharp — server is not reachable. " +
                        $"SocketException: {ex.Message}");
            return;
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.That(contentType, Is.EqualTo("text/plain"),
            $"GET /sdk/csharp content-type was '{contentType}', expected 'text/plain'");
    }
}
