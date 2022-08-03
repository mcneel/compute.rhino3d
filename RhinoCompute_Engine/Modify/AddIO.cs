using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AddIO(this GrasshopperDefinition rc)
        {
            IList<IGH_DocumentObject> documentObjects = rc.GH_Document.Objects;

            foreach (IGH_DocumentObject docObj in documentObjects)
                rc.AddIO(docObj);
        }

        private static void AddIO(this GrasshopperDefinition rc, IGH_DocumentObject docObj)
        {
            if (docObj.IsRemoteInput())
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Input[0];
                rc.AddInput(param, docObj.RemoteInputName(), docObj.Description());
                return;
            }

            if (docObj.IsRemoteOutput())
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Output[0];
                rc.AddOutput(param, docObj.RemoteOutputName(), docObj.Description());
                return;
            }

            IGH_ContextualParameter contextualParam = docObj as IGH_ContextualParameter;
            if (contextualParam != null)
            {
                IGH_Param param = docObj as IGH_Param;
                if (param != null)
                    rc.AddInput(param, param.NickName, param.Description);

                return;
            }

            Type docObjType = docObj.GetType();
            var className = docObjType.Name;
            if (className == "ContextBakeComponent" || className == "ContextPrintComponent")
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Input[0];
                rc.AddOutput(param, param.NickName);
            }

            GH_Group group = docObj as GH_Group;
            if (group == null)
                return;

            string groupName = group.NickName;
            var groupObjects = group.Objects();

            if (groupName.Contains(m_inputGroupKey) && groupObjects.Count > 0)
            {
                string inputName = groupName.Replace(m_inputGroupKey, "");
                var param = groupObjects[0] as IGH_Param;
                if (param != null)
                    rc.AddInput(param, inputName, param.Description());
            }

            if (groupName.Contains(m_outputGroupKey) && groupObjects.Count > 0)
            {
                string outputName = groupName.Replace(m_outputGroupKey, "");

                if (groupObjects[0] is IGH_Param param)
                {
                    rc.AddOutput(param, outputName);
                }
                else if (groupObjects[0] is GH_Component component)
                {
                    int outputCount = component.Params.Output.Count;
                    for (int i = 0; i < outputCount; i++)
                    {
                        if (1 == outputCount)
                        {
                            rc.AddOutput(component.Params.Output[i], outputName);
                        }
                        else
                        {
                            string itemName = $"{outputName} ({component.Params.Output[i].NickName})";
                            rc.AddOutput(component.Params.Output[i], itemName);
                        }
                    }
                }
            }
        }

        private static string m_inputGroupKey = "RH_IN:";
        private static string m_outputGroupKey = "RH_OUT:";
    }
}
