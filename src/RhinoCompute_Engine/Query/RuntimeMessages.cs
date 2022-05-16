using Grasshopper.Kernel;
using System.Collections.Generic;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static void RuntimeMessages(this GH_Document gh_document, out List<string> errors, out List<string> warnings, out List<string> remarks)
        {
            List<IGH_ActiveObject> activeObjects = gh_document.ActiveObjects();

            errors = new List<string>();
            warnings = new List<string>();
            remarks = new List<string>();

            if (activeObjects == null)
                return;

            errors = RuntimeMessages(activeObjects, GH_RuntimeMessageLevel.Error);
            warnings = RuntimeMessages(activeObjects, GH_RuntimeMessageLevel.Warning);
            remarks = RuntimeMessages(activeObjects, GH_RuntimeMessageLevel.Remark);
        }

        public static List<string> RuntimeMessages(IEnumerable<IGH_ActiveObject> objs, GH_RuntimeMessageLevel messageLevel)
        {
            List<string> allObjsMessages = new List<string>();

            foreach (var obj in objs)
                allObjsMessages.AddRange(RuntimeMessages(obj, messageLevel));

            return allObjsMessages;
        }

        public static List<string> RuntimeMessages(IGH_ActiveObject obj, GH_RuntimeMessageLevel messageLevel, bool skipFirstNullErrorForBHoMComponents = true)
        {
            List<string> runtimeMessages = new List<string>();

            IList<string> messages = obj.RuntimeMessages(messageLevel);

            // For some reason, all BHoM components return the following error, even if their computation works.
            // For now, we avoid reporting the first occurrence of this error on BHoM components. 
            // This obviously brings risks as we could miss to report true positives.
            // TODO: discover why.
            string nullErrorText = "Solution exception:Object reference not set to an instance of an object.";

            foreach (var msg in messages)
            {
                if (skipFirstNullErrorForBHoMComponents && obj.GetType().FullName.StartsWith("BH.UI") && msg == nullErrorText)
                {
                    skipFirstNullErrorForBHoMComponents = false;
                    continue;
                }

                runtimeMessages.Add($"{messageLevel} message from component named `{obj.Name}`, instance GUID `{obj.InstanceGuid}`, type {obj.GetType().FullName}:\n\t{msg}");
            }

            return runtimeMessages;
        }
    }
}
