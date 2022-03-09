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
            var component = Grasshopper.Instances.ComponentServer.EmitObject(componentId) as GH_Component;
            if (component == null)
                return null;

            var gh_document = new GH_Document();
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

            GrasshopperDefinition rc = new GrasshopperDefinition(gh_document, null);
            rc._singularComponent = component;

            foreach (var input in component.Params.Input)
            {
                rc._input[input.NickName] = new InputGroup(input);
            }

            foreach (var output in component.Params.Output)
            {
                rc._output[output.NickName] = output;
            }

            return rc;
        }

        // --------------------------------------------------------------------- //

        private static GrasshopperDefinition Construct(GH_Archive archive)
        {
            string icon = null;
            var chunk = archive.GetRootNode.FindChunk("Definition");
            if (chunk != null)
            {
                chunk = chunk.FindChunk("DefinitionProperties");
                if (chunk != null)
                {
                    string s = String.Empty;
                    if (chunk.TryGetString("IconImageData", ref s))
                    {
                        icon = s;
                    }
                }
            }

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
                throw new Exception("Unable to extract definition from archive");

            try
            {
                // raise DocumentServer.DocumentAdded event (used by some plug-ins)
                Grasshopper.Instances.DocumentServer.AddDocument(definition);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception in DocumentAdded event handler");
            }

            GrasshopperDefinition rc = new GrasshopperDefinition(definition, icon);
            foreach (var obj in definition.Objects)
            {
                IGH_ContextualParameter contextualParam = obj as IGH_ContextualParameter;
                if (contextualParam != null)
                {
                    IGH_Param param = obj as IGH_Param;
                    if (param != null)
                    {
                        AddInput(rc, param, param.NickName);
                    }
                    continue;
                }


                Type objectClass = obj.GetType();
                var className = objectClass.Name;
                if (className == "ContextBakeComponent")
                {
                    var contextBaker = obj as GH_Component;
                    IGH_Param param = contextBaker.Params.Input[0];
                    AddOutput(ref rc, param, param.NickName);
                }

                if (className == "ContextPrintComponent")
                {
                    var contextPrinter = obj as GH_Component;
                    IGH_Param param = contextPrinter.Params.Input[0];
                    AddOutput(ref rc, param, param.NickName);
                }

                var group = obj as GH_Group;
                if (group == null)
                    continue;

                string nickname = group.NickName;
                var groupObjects = group.Objects();
                if (nickname.Contains("RH_IN") && groupObjects.Count > 0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null)
                    {
                        AddInput(rc, param, nickname);
                    }
                }

                if (nickname.Contains("RH_OUT") && groupObjects.Count > 0)
                {
                    if (groupObjects[0] is IGH_Param param)
                    {
                        AddOutput(rc, param, nickname);
                    }
                    else if (groupObjects[0] is GH_Component component)
                    {
                        int outputCount = component.Params.Output.Count;
                        for (int i = 0; i < outputCount; i++)
                        {
                            if (1 == outputCount)
                            {
                                AddOutput(rc, component.Params.Output[i], nickname);
                            }
                            else
                            {
                                string itemName = $"{nickname} ({component.Params.Output[i].NickName})";
                                AddOutput(rc, component.Params.Output[i], itemName);
                            }
                        }
                    }
                }
            }
            return rc;
        }
    }
}
