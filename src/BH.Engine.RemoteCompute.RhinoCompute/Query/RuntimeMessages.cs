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

        public static List<string> RuntimeMessages(IGH_ActiveObject obj, GH_RuntimeMessageLevel messageLevel)
        {
            List<string> runtimeMessages = new List<string>();

            foreach (var msg in obj.RuntimeMessages(messageLevel))
                runtimeMessages.Add($"{messageLevel} message from component \"{obj.Name}\" ({obj.InstanceGuid}):\n\t{msg}");

            return runtimeMessages;
        }
    }
}
