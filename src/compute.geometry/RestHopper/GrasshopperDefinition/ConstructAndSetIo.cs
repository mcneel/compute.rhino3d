using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RhinoCompute;

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
using BH.Engine.RemoteCompute.RhinoCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        private static GrasshopperDefinition ConstructAndSetIo(GH_Archive archive)
        {
            GH_Document ghDocument = archive.GHDocument();

            asdasd
                /* TODOS:
                 * 1) Implement RemoteComputeInputs/Outputs extraction from deserialized GH_Document directly in RemoteCompute_Prototypes solution,
                 *    so that we can reuse the same functionality to provide the User with needed inputs and expected outputs.
                 * 2) Reference such functionality in compute.geometry solution
                 * 3) Test I/O
                */


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
            SetIO(rc);

            string iconImageData = null;
            var chunk = archive.GetRootNode.FindChunk("Definition");
            if (chunk != null)
            {
                chunk = chunk.FindChunk("DefinitionProperties");
                if (chunk != null)
                {
                    chunk.TryGetString("IconImageData", ref iconImageData);
                }
            }

            return rc;
        }

        // --------------------------------------------------------------------- //

        private static GrasshopperDefinition Construct(Guid componentId)
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
