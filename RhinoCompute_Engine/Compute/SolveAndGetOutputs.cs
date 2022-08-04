using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Log = BH.Engine.RemoteCompute.Log;
using System;
using System.Linq;
using Grasshopper;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Compute
    {
        public static ResthopperOutputs SolveAndGetOutputs(this GrasshopperDefinition ghDef, GHScriptConfig gHScriptConfig = null, bool logAllMessages = true)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            ghDef.SolveDefinition();
            ResthopperOutputs resthopperOutputs = ghDef.ResthopperOutputs();

            if (logAllMessages)
                ghDef.LogAllMessages();

            // Close and clean.
            if (gHScriptConfig.RaiseDocumentOpenEvent && Grasshopper.Instances.DocumentServer.GetEnumerator_Generic().ToList().Contains(ghDef.GH_Document))
                Grasshopper.Instances.DocumentServer.RemoveDocument(ghDef.GH_Document);

            ghDef.GH_Document.CloseAllSubsidiaries();
            ghDef.GH_Document.Dispose();

            return resthopperOutputs;
        }

        private static void LogAllMessages(this GrasshopperDefinition ghDef)
        {
            // Get Runtime messages from GH Document 
            var runtimeMessages = ghDef.GH_Document.RuntimeMessages();

            // Add the messages to the log. Depending on the runtime context, this may or may not also raise them to the UI.
            runtimeMessages.Errors.ForEach(m => Log.RecordError(m));
            runtimeMessages.Warnings.ForEach(m => Log.RecordWarning(m));
            runtimeMessages.Remarks.ForEach(m => Log.RecordNote(m));
        }
    }
}
