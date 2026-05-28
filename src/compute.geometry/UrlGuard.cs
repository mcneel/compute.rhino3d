using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace compute.geometry
{
    // SSRF + size-cap protection for server-side URL fetches. All HTTP fetches
    // initiated by compute.geometry (GrasshopperDefinition.ArchiveFromUrl and
    // DataCache) go through this guard so they get:
    //   * scheme validation — only http/https allowed; blocks file://, javascript://,
    //     ftp:// and any other scheme that has no business fetching a .gh file
    //   * optional private-IP blocking — when Config.BlockPrivateUrls is true,
    //     refuses any URL whose hostname DNS-resolves to a private/loopback/
    //     link-local IP. Protects cloud-hosted servers from SSRF attacks against
    //     cloud metadata endpoints (169.254.169.254 on AWS/Azure/GCP) and
    //     internal LAN services
    //   * streaming response read with a size cap from Config.MaxRequestSize so
    //     a malicious server hosting a multi-GB body can't exhaust memory
    static class UrlGuard
    {
        // Lightweight classifier: true only if the string is a well-formed ABSOLUTE
        // http/https URL. Prefer this over a StartsWith("http") prefix check, which is
        // both too loose and not a real parse — it accepts look-alikes like "httpfoo://"
        // or a relative path named "httpdocs/model.gh". Used to decide whether a path
        // should be fetched over HTTP vs treated as a local file. (Validate() enforces the
        // same scheme rule but throws on violation; use IsWebUrl where a bool fits.)
        public static bool IsWebUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        // Throws an exception describing the violation if the URL is malformed,
        // uses a non-http(s) scheme, or (when Config.BlockPrivateUrls is true)
        // resolves to a private/loopback IP. Returns normally on success.
        public static void Validate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL must not be empty.");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException($"Invalid URL: {url}");

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException(
                    $"URL scheme '{uri.Scheme}' not allowed; only http and https are permitted.");

            if (!Config.BlockPrivateUrls)
                return;

            IPAddress[] addresses;
            try
            {
                addresses = Dns.GetHostAddresses(uri.Host);
            }
            catch (SocketException ex)
            {
                throw new ArgumentException($"DNS lookup failed for '{uri.Host}': {ex.Message}");
            }

            foreach (var addr in addresses)
            {
                if (IsBlockedAddress(addr))
                    throw new ArgumentException(
                        $"URL host '{uri.Host}' resolves to {addr}, which is a private, loopback, " +
                        $"or link-local address. Disable --block-private-urls (or unset " +
                        $"RHINO_COMPUTE_BLOCK_PRIVATE_URLS) to allow internal fetches.");
            }
        }

        // Validates the URL, then fetches the response body via streaming read so
        // the download aborts as soon as we exceed maxBytes. The server's
        // Content-Length is checked upfront when available so we can refuse
        // before reading; we still enforce during the read in case the server
        // lies about the length or omits the header.
        public static async Task<byte[]> GetByteArrayAsync(string url, long maxBytes, CancellationToken ct = default)
        {
            Validate(url);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await HttpClientHelper.Client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);
            // Surface the source server's own status in a clean message rather than the
            // generic EnsureSuccessStatusCode text ("Response status code does not indicate
            // success: ..."), so callers can show the user exactly what the remote URL returned.
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"The server responded with {(int)response.StatusCode} ({response.StatusCode}).");

            if (response.Content.Headers.ContentLength is long len && len > maxBytes)
                throw new InvalidOperationException(
                    $"Response body Content-Length ({len} bytes) exceeds the configured " +
                    $"maximum request size ({maxBytes} bytes).");

            using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var ms = new MemoryStream();
            var buffer = new byte[81920];
            long total = 0;
            int read;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
            {
                total += read;
                if (total > maxBytes)
                    throw new InvalidOperationException(
                        $"Response body exceeded the configured maximum request size " +
                        $"({maxBytes} bytes) during streaming read.");
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        public static async Task<string> GetStringAsync(string url, long maxBytes, CancellationToken ct = default)
        {
            var bytes = await GetByteArrayAsync(url, maxBytes, ct).ConfigureAwait(false);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        // Returns true if the address is in any range that is not publicly routable —
        // loopback, private LAN, link-local, multicast, etc. Covers IPv4 + IPv6.
        // Note on TOCTOU: a hostile server could be DNS-resolved to a public IP at
        // Validate() time and a private IP when HttpClient actually connects. Closing
        // that hole would require a custom SocketsHttpHandler.ConnectCallback. Out of
        // scope for this pass — the current check is still useful for the common SSRF
        // patterns where the attacker controls the URL but not DNS.
        static bool IsBlockedAddress(IPAddress addr)
        {
            if (IPAddress.IsLoopback(addr)) return true;
            if (addr.Equals(IPAddress.Any) || addr.Equals(IPAddress.IPv6Any)) return true;

            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = addr.GetAddressBytes();
                if (b[0] == 10) return true;                              // 10.0.0.0/8
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true; // 172.16.0.0/12
                if (b[0] == 192 && b[1] == 168) return true;              // 192.168.0.0/16
                if (b[0] == 169 && b[1] == 254) return true;              // 169.254.0.0/16 link-local (cloud metadata)
                if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return true;// 100.64.0.0/10 carrier-grade NAT
                if (b[0] == 0) return true;                               // 0.0.0.0/8 this network
                if (b[0] >= 224) return true;                             // 224.0.0.0/4 multicast + 240.0.0.0/4 reserved
            }
            else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (addr.IsIPv6LinkLocal) return true;
                if (addr.IsIPv6SiteLocal) return true;
                if (addr.IsIPv6Multicast) return true;
                var b = addr.GetAddressBytes();
                if ((b[0] & 0xFE) == 0xFC) return true; // fc00::/7 unique local addresses
            }

            return false;
        }
    }
}
