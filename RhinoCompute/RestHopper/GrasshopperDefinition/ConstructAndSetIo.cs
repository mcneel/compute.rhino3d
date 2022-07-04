using System;
using Grasshopper.Kernel;
using GH_IO.Serialization;
using Serilog;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinitionUtils
    {
        private static GrasshopperDefinition ConstructAndSetIo(GH_Archive archive)
        {
            GH_Document ghDocument = archive.GHDocument();

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(ghDocument);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception in DocumentAdded event handler");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(ghDocument);

            // Set inputs and outputs.
            rc.SetIO();

            return rc;
        }

        // --------------------------------------------------------------------- //

        private static GrasshopperDefinition ConstructAndSetIo(Guid componentId)
        {
            GH_Component component = Grasshopper.Instances.ComponentServer.EmitObject(componentId) as GH_Component;
            if (component == null)
                return null;

            GH_Document gh_document = new GH_Document();
            gh_document.AddObject(component, false);

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(gh_document);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception in DocumentAdded event handler");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(gh_document);
            rc.SingularComponent = component;

            foreach (var input in component.Params.Input)
            {
                rc.Inputs[input.NickName] = new InputGroup(input);
            }

            foreach (var output in component.Params.Output)
            {
                rc.Outputs[output.NickName] = output;
            }

            return rc;
        }
    }
}
