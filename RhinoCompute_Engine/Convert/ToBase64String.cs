
using BH.oM.Computing.RhinoCompute;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Convert
    {
        public static string ToBase64String(this GH_Document ghDoc)
        {
            var gh_archive = ghDoc.ToGHArchive();
            var base64string = gh_archive.ToBase64String();

            return base64string;
        }

        public static string ToBase64String(this GrasshopperDefinition ghDef)
        {
            var base64string = ghDef.GH_Document.ToBase64String();

            return base64string;
        }

        public static string ToBase64String(this GH_Archive ghArchive)
        {
            byte[] binary = ghArchive.Serialize_Binary();
            string base64String = System.Convert.ToBase64String(binary);

            return base64String;
        }
    }
}
