using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Computing;
using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.Computing.RhinoCompute
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

            // Kept for compatibility with existing non-BHoM RhinoCompute scripts.
            if (groupName.StartsWith("RH_IN:"))
            {
                string inputName = groupName.Replace("RH_IN:", "");
                var param = groupObjects[0] as IGH_Param;
                if (param != null)
                    ghDef.AddInput(param, inputName, param.Description());
            }

            // Kept for compatibility with existing non-BHoM RhinoCompute scripts.
            if (groupName.StartsWith("RH_OUT:"))
            {
                string outputName = groupName.Replace("RH_OUT:", "");

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

            // Deal with multiple "INPUT" components included in a group.
            if (groupName.StartsWith(ghscriptconfig.InputGroupNamePrefix))
            {
                Log.RecordNote($"Gathering inputs from group named `{groupName}`.");

                foreach (IGH_DocumentObject groupObj in groupObjects)
                {
                    var param = groupObj as IGH_Param;
                    if (param != null)
                        ghDef.AddInput(param, param.NickName, param.Description());
                }
            }

            // Deal with multiple "TRIGGER" components included in a group.
            if (groupName.StartsWith(ghscriptconfig.TriggerGroupNamePrefix))
            {
                Log.RecordNote($"Gathering triggers from group named `{groupName}`.");

                string triggerIndexStr = groupName.Replace(ghscriptconfig.TriggerGroupNamePrefix, "").Trim();
                int triggerIndex = (int.TryParse(triggerIndexStr, out triggerIndex) && triggerIndex > 0) ? triggerIndex : -1;

                foreach (IGH_DocumentObject groupObj in groupObjects)
                {
                    if (triggerIndex == -1)
                    {
                        ghDef.Triggers.Add(ghDef.Triggers.Keys.LastOrDefault() + 1, groupObj);
                        continue;
                    }

                    if (ghDef.Triggers.Keys.Contains(triggerIndex))
                    {
                        Log.RecordWarning($"The trigger group `{groupName}` specifies an index number ({triggerIndex}) that already used in another trigger group. It will be triggered last.");
                        ghDef.Triggers.Add(ghDef.Triggers.Keys.LastOrDefault(), groupObj);
                    }

                    ghDef.Triggers[triggerIndex] = groupObj;
                }
            }
        }
    }
}
