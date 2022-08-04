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
        public static List<RemoteOutputInstance> RunScript(string scriptFilePath, List<IObject> inputs = null, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            List<RemoteOutputInstance> result = new List<RemoteOutputInstance>();

            if (!scriptFilePath.IsExistingGhFile())
                return new List<RemoteOutputInstance>();

            if (!active)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please set the `{nameof(active)}` input to true to activate the computation.");
                return new List<RemoteOutputInstance>();
            }

            var ghDocumentIo = new GH_DocumentIO();
            ghDocumentIo.Open(scriptFilePath);

            GH_Document ghDoc = ghDocumentIo.Document;
            if (ghDoc == null)
            {
                Log.RecordError("Could not extract a Grasshopper definition from the input file.");
                return new List<RemoteOutputInstance>();
            }

            GrasshopperDefinition ghDef = ghDoc.ToGrasshopperDefinition(gHScriptConfig);

            // Set inputs
            List<RemoteInputInstance> allInputsProvided = inputs.ToRemoteInputData();
            List<ResthopperInputTree> allResthopperInputs = allInputsProvided.FromBHoM();
            ghDef.SetInputsData(allResthopperInputs);

            // Solve the GrasshopperDefinition.
            ResthopperOutputs outputSchema = ghDef.SolveAndGetOutputs();

            Log.RaiseAllMessagesToUI();
            Log.Clean();

            return outputSchema.ToBHoM();
        }
    }
}
