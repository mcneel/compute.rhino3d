using System;
using System.IO;
using System.Net;
using GH_IO.Serialization;
using BH.Engine.RemoteCompute.RhinoCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Compute
    {
        public static GH_Archive GHArchiveFromFilepath(this string filePath)
        {
            byte[] byteArray = File.ReadAllBytes(filePath);

            var byteArchive = new GH_Archive();

            if (byteArchive.Deserialize_Binary(byteArray))
                return byteArchive;

            string Xml = System.Text.Encoding.UTF8.GetString(byteArray).StripBom();

            var xmlArchive = new GH_Archive();
            if (xmlArchive.Deserialize_Xml(Xml))
                return xmlArchive;

            return null;
        }
    }
}
