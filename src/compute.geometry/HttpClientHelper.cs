using System.Net;
using System.Net.Http;

namespace compute.geometry
{
    /// <summary>
    /// Shared HttpClient instance for server-side HTTP fetches (e.g. downloading a Grasshopper
    /// definition from a URL, fetching cached JSON). Replaces the deprecated WebClient and
    /// HttpWebRequest patterns. GZip/Deflate decompression is enabled so callers receive
    /// already-decompressed payloads, matching the prior HttpWebRequest behavior.
    /// </summary>
    static class HttpClientHelper
    {
        static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    });
                }
                return _client;
            }
        }
    }
}
