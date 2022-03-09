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
        private static void SetIOFromGHDocumentObjects(GrasshopperDefinition rc, IList<IGH_DocumentObject> documentObjects)
        {
            foreach (IGH_DocumentObject docObj in documentObjects)
                SetIOFromGHDocumentObject(rc, docObj);
        }

        private static void SetIOFromGHDocumentObject(GrasshopperDefinition rc, IGH_DocumentObject docObj)
        {
            IGH_ContextualParameter contextualParam = docObj as IGH_ContextualParameter;
            if (contextualParam != null)
            {
                IGH_Param param = docObj as IGH_Param;
                if (param != null)
                    AddInput(rc, param, param.NickName);

                return;
            }

            Type docObjType = docObj.GetType();
            var className = docObjType.Name;
            if (className == "ContextBakeComponent")
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Input[0];
                AddOutput(rc, param, param.NickName);
            }

            if (className == "ContextPrintComponent")
            {
                var contextPrinter = docObj as GH_Component;
                IGH_Param param = contextPrinter.Params.Input[0];
                AddOutput(rc, param, param.NickName);
            }

            GH_Group group = docObj as GH_Group;
            if (group == null)
                return;

            string groupName = group.NickName;
            var groupObjects = group.Objects();
            if (groupName.Contains("RH_IN") && groupObjects.Count > 0)
            {
                var param = groupObjects[0] as IGH_Param;
                if (param != null)
                {
                    AddInput(rc, param, groupName);
                }
            }

            if (groupName.Contains("RH_OUT") && groupObjects.Count > 0)
            {
                if (groupObjects[0] is IGH_Param param)
                {
                    AddOutput(rc, param, groupName);
                }
                else if (groupObjects[0] is GH_Component component)
                {
                    int outputCount = component.Params.Output.Count;
                    for (int i = 0; i < outputCount; i++)
                    {
                        if (1 == outputCount)
                        {
                            AddOutput(rc, component.Params.Output[i], groupName);
                        }
                        else
                        {
                            string itemName = $"{groupName} ({component.Params.Output[i].NickName})";
                            AddOutput(rc, component.Params.Output[i], itemName);
                        }
                    }
                }
            }
        }
    }
}
