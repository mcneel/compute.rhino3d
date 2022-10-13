using System;
using System.IO;
using System.Net;
using GH_IO.Serialization;
using BH.Engine.Computing.RhinoCompute;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Compute
    {
        /// <summary>
        /// Performs a GET request to the specified url, then tries to decompress and deserialize the response.
        /// </summary>
        public static GH_Archive GHArchiveFromUrl(Uri uri)
        {
            string url = uri.ToString();
            if (url == null || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return null;

            GH_Archive ghArchive = new GH_Archive();

            // Try geting from byte array.
            byte[] byteArray = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                byteArray = memStream.ToArray();
            }

            if (ghArchive.Deserialize_Binary(byteArray))
                return ghArchive;

            // Try geting from Xml.
            string grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray).StripBom();
            if (ghArchive.Deserialize_Xml(grasshopperXml))
                return ghArchive;

            return null;
        }
    }
}
