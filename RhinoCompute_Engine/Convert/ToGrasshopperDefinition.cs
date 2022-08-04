
using System;
using System.Collections.Generic;
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
            if (gHScriptConfig.RaiseDocumentOpenEvent)
                try
                {
                    // Raise DocumentServer.DocumentAdded event (used by some plug-ins).
                    // NOTE: If this is done, the script also needs to be removed from the DocumentServer after it is used, with:
                    // DocumentServer.RemoveDocument(ghDocument);
                    // Otherwise the DocumentServer becomes cluttered. Despite what the AddDocument() function documentation says,
                    // the same document is added multiple times without checking if it is already registered.
                    // This results into a cluttered document selector in the UI if this function is called locally.
                    // We _could_ check if the document is already openend with:
                    // Grasshopper.Instances.DocumentServer.GetEnumerator_Generic().ToList().Contains(ghDocument);
                    // however we still may want to be raising the `DocumentAdded` event even if it is already registered.
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
