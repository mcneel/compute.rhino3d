
using System;
using BH.oM.RemoteCompute.RhinoCompute;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static GrasshopperDefinition ToGrasshopperDefinition(this GH_Archive archive)
        {
            GH_Document ghDocument = archive.GHDocument();

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(ghDocument);
            }
            catch (Exception e)
            {
                BH.Engine.RemoteCompute.Log.RecordWarning($"Exception in DocumentAdded event handler:\n\t{e.Message}");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(ghDocument);

            return rc;
        }
    }
}
