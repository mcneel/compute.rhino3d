
using System;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static GrasshopperDefinition ToGrasshopperDefinition(this string base64String, GHScriptConfig gHScriptConfig)
        {
            GH_Archive archive = base64String.GHArchiveFromBase64String();
            if (archive == null)
                return null;

            GrasshopperDefinition gDef = archive.ToGrasshopperDefinition(gHScriptConfig);

            // Set inputs and outputs.
            gDef.AddIO();

            return gDef;
        }

        public static GrasshopperDefinition ToGrasshopperDefinition(this GH_Archive archive, GHScriptConfig gHScriptConfig)
        {
            GH_Document ghDocument = archive.GHDocument();

            return ghDocument.ToGrasshopperDefinition(gHScriptConfig);
        }

        public static GrasshopperDefinition ToGrasshopperDefinition(this GH_Document ghDocument, GHScriptConfig gHScriptConfig)
        {
            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(ghDocument);
            }
            catch (Exception e)
            {
                BH.Engine.RemoteCompute.Log.RecordWarning($"Exception in DocumentAdded event handler:\n\t{e.Message}");
            }

            GrasshopperDefinition gDef = new GrasshopperDefinition(ghDocument, gHScriptConfig);

            // Set inputs and outputs.
            gDef.AddIO();

            return gDef;
        }
    }
}
