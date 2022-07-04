
using GH_IO.Serialization;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static GH_Archive GHArchiveFromBase64String(this string base64string)
        {
            if (string.IsNullOrWhiteSpace(base64string))
                return null;

            byte[] byteArray = System.Convert.FromBase64String(base64string);

            return Query.GHArchive(byteArray);
        }
    }
}
