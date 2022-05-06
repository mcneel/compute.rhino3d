using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

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

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
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

        // --------------------------------------------------------------------- //

        private static GrasshopperDefinition Construct(GH_Archive archive)
        {
            GH_Document ghDocument = new GH_Document();
            if (!archive.ExtractObject(ghDocument, "Definition"))
                throw new Exception("Unable to extract definition from archive");

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
    }
}
