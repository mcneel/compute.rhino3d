
using GH_IO.Serialization;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static GH_Archive ArchiveFromBase64String(this string base64string)
        {
            if (string.IsNullOrWhiteSpace(base64string))
                return null;

            byte[] byteArray = System.Convert.FromBase64String(base64string);

            return GHArchive(byteArray);
        }
    }
}
