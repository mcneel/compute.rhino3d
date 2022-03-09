using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public static GH_Archive ArchiveFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (File.Exists(url))
            {
                // local file
                var archive = new GH_Archive();
                if (archive.ReadFromFile(url))
                {
                    RegisterFileWatcher(url);
                    return archive;
                }
                return null;
            }

            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
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

                try
                {
                    var byteArchive = new GH_Archive();
                    if (byteArchive.Deserialize_Binary(byteArray))
                        return byteArchive;
                }
                catch (Exception) { }

                var grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray).StripBom();
                var xmlArchive = new GH_Archive();
                if (xmlArchive.Deserialize_Xml(grasshopperXml))
                    return xmlArchive;
            }
            return null;
        }
    }
}
