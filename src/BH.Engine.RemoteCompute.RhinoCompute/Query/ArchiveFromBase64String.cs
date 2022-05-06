
using GH_IO.Serialization;
using BH.Engine.RhinoCompute;
using System.Linq;
using System.Reflection;
using BH.Engine.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    public static partial class Query
    {
        public static GH_Archive ArchiveFromBase64String(this string base64string)
        {
            if (string.IsNullOrWhiteSpace(base64string))
                return null;

            byte[] byteArray = System.Convert.FromBase64String(base64string);

            return BH.Engine.RemoteCompute.RhinoCompute.Query.GHArchive(byteArray);
        }
    }
}
