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
using System.IO;
using BH.oM.Base;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Compute
    {
        public static List<RemoteOutputInstance> RunScript(string scriptFilePath, List<IObject> inputs = null, bool active = false)
        {
            List<RemoteOutputInstance> result = new List<RemoteOutputInstance>();

            if (!LocalGHFileExists(scriptFilePath))
                return new List<RemoteOutputInstance>();

            if (!active)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please set the `{nameof(active)}` input to true to activate the computation.");
                return new List<RemoteOutputInstance>();
            }

            var io = new GH_DocumentIO();
            io.Open(scriptFilePath);

            GH_Document ghDoc = io.Document;
            if (ghDoc == null)
            {
                Log.RecordError("Could not extract a Grasshopper definition from the input file.");
                return new List<RemoteOutputInstance>();
            }

            // Set inputs
            foreach (IGH_DocumentObject docObj in ghDoc.Objects)
            {
                if (!docObj.IsRemoteInput())
                    continue;

                var paramInputs = (docObj as dynamic).Params.Input;
                if (paramInputs == null)
                    continue;

                IGH_Param outputDataParam = paramInputs[0];
                IGH_Param outputDescriptionParam = (docObj as dynamic).Params.Input[1];
                if (outputDataParam == null)
                    continue;

                string remoteInputName = docObj.RemoteInputName();
                RemoteInputInstance inputInstance = inputs.ToRemoteInputData().Where(i => i.Name == remoteInputName).FirstOrDefault();

                Grasshopper.Utility.InvokeMethod(outputDataParam, "Script_ClearPersistentData");
                Grasshopper.Utility.InvokeMethod(outputDataParam, "Script_AddPersistentData", inputInstance.Data.SimplifiedListOfLists());
            }

            // Compute
            ghDoc.Enabled = true;
            ghDoc.NewSolution(true, GH_SolutionMode.Silent);

            // Get and log computation messages
            List<string> errors, warnings, remarks = new List<string>();
            ghDoc.RuntimeMessages(out errors, out warnings, out remarks);

            errors?.ForEach(m => Log.RecordError(m));
            warnings?.ForEach(m => Log.RecordWarning(m));
            remarks?.ForEach(m => Log.RecordNote(m));

            // Set outputs
            foreach (IGH_DocumentObject obj in ghDoc.Objects)
            {
                if (!obj.IsRemoteOutput())
                    continue;

                var paramOutputs = (obj as dynamic).Params.Output;
                if (paramOutputs == null)
                    continue;

                IGH_Param outputDataParam = paramOutputs[0];
                IGH_Param outputDescriptionParam = (obj as dynamic).Params.Input[1];
                if (outputDataParam == null)
                    continue;

                RemoteOutputInstance remoteOutputInstance = new RemoteOutputInstance();

                // Get data
                IGH_Structure volatileData = outputDataParam.VolatileData;
                var outputListOfLists = volatileData.ToListOfLists();

                remoteOutputInstance.Name = outputDataParam.NickName;
                remoteOutputInstance.Data = outputListOfLists.SimplifiedListOfLists();
                remoteOutputInstance.DefaultValue = outputDataParam.DefaultValue();
                remoteOutputInstance.Description = outputDescriptionParam.Description(true);

                result.Add(remoteOutputInstance);
            }

            ghDoc.Dispose();

            return result;
        }

        private static bool LocalGHFileExists(string ghFilePath)
        {
            if (ghFilePath == null || string.IsNullOrWhiteSpace(ghFilePath))
            {
                Log.RecordError($"Missing script filepath.");
                return false;
            }

            if (!ghFilePath.EndsWith(".gh"))
            {
                Log.RecordError($"File `{nameof(ghFilePath)}` is not a `.gh` script.");
                return false;
            }

            if (!File.Exists(ghFilePath))
            {
                Log.RecordError($"File `{nameof(ghFilePath)}` could not be found on disk.");
                return false;
            }

            return true;
        }
    }
}
