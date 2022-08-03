using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AddIO(this GrasshopperDefinition ghDef)
        {
            IList<IGH_DocumentObject> documentObjects = ghDef.GH_Document.Objects;

            foreach (IGH_DocumentObject docObj in documentObjects)
                ghDef.AddIO(docObj);
        }

        private static void AddIO(this GrasshopperDefinition ghDef, IGH_DocumentObject docObj)
        {
            if (docObj.IsRemoteInput())
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Input[0];
                ghDef.AddInput(param, docObj.RemoteInputName(), docObj.Description());
                return;
            }

            if (docObj.IsRemoteOutput())
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Output[0];
                ghDef.AddOutput(param, docObj.RemoteOutputName(), docObj.Description());
                return;
            }

            // Not sure about this ContextualParameter IO handling.
            // Left for compatibility with non-BHoM RhinoCompute scripts.
            IGH_ContextualParameter contextualParam = docObj as IGH_ContextualParameter;
            if (contextualParam != null)
            {
                IGH_Param param = docObj as IGH_Param;
                if (param != null)
                    ghDef.AddInput(param, param.NickName, param.Description);

                return;
            }

            // Not sure about this ContextBakeComponent/ContextPrintComponent IO handling.
            // Left for compatibility with non-BHoM RhinoCompute scripts.
            Type docObjType = docObj.GetType();
            var className = docObjType.Name;
            if (className == "ContextBakeComponent" || className == "ContextPrintComponent")
            {
                var contextBaker = docObj as GH_Component;
                IGH_Param param = contextBaker.Params.Input[0];
                ghDef.AddOutput(param, param.NickName);

                return;
            }

            // Handle inputs/outputs specified as Groups of components.
            ghDef.AddIOsFromGroups(docObj as GH_Group, ghDef.GHScriptConfig);
        }

        private static void AddIOsFromGroups(this GrasshopperDefinition ghDef, GH_Group group, GHScriptConfig ghscriptconfig)
        {
            if (group == null)
                return;

            string groupName = group.NickName;
            var groupObjects = group.Objects();

            if (groupObjects.Count <= 0)
                return;

            if (groupName.StartsWith(ghscriptconfig.InputSingleComponentGroupName))
            {
                string inputName = groupName.Replace(ghscriptconfig.InputSingleComponentGroupName, "");
                var param = groupObjects[0] as IGH_Param;
                if (param != null)
                    ghDef.AddInput(param, inputName, param.Description());
            }

            if (groupName.StartsWith(ghscriptconfig.OutputSingleComponentGroupName))
            {
                string outputName = groupName.Replace(ghDef.GHScriptConfig.OutputSingleComponentGroupName, "");

                if (groupObjects[0] is IGH_Param param)
                {
                    ghDef.AddOutput(param, outputName);
                }
                else if (groupObjects[0] is GH_Component component)
                {
                    int outputCount = component.Params.Output.Count;
                    for (int i = 0; i < outputCount; i++)
                    {
                        if (1 == outputCount)
                        {
                            ghDef.AddOutput(component.Params.Output[i], outputName);
                        }
                        else
                        {
                            string itemName = $"{outputName} ({component.Params.Output[i].NickName})";
                            ghDef.AddOutput(component.Params.Output[i], itemName);
                        }
                    }
                }
            }

            if (groupName.ToLower() == ghscriptconfig.InputMultipleComponentsGroupName.ToLower())
            {
                Log.RecordNote($"Gathering inputs from group named `{groupName}`.");

                foreach (IGH_DocumentObject groupObj in groupObjects)
                {
                    var param = groupObj as IGH_Param;
                    if (param != null)
                        ghDef.AddInput(param, param.NickName, param.Description());
                }
            }
        }
    }
}
