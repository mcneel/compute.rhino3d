using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using BH.oM.Base;
using BH.oM.RemoteCompute;
using BH.Engine.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Log = BH.Engine.RemoteCompute.Log;
using BH.oM.Base.Attributes;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Compute
    {
        private static bool m_PartOfChain = false;

        [MultiOutputAttribute(0, "Logs", "Log returned by each script.")]
        [MultiOutputAttribute(1, "Outputs", "Outputs returned by each script.")]
        public static Output<List<RuntimeMessages>, List<List<RemoteOutputData>>> RunScripts(List<string> scriptFilePaths, List<CustomObject> inputs = null, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            var emptyOutput = new Output<List<RuntimeMessages>, List<List<RemoteOutputData>>>() { Item1 = new List<RuntimeMessages>(), Item2 = new List<List<RemoteOutputData>>() };

            if (scriptFilePaths.Any() && inputs.Any() && scriptFilePaths.Count != inputs.Count)
            {
                BH.Engine.Base.Compute.RecordError($"Specified {scriptFilePaths.Count} {nameof(scriptFilePaths)} and {inputs.Count} {nameof(inputs)}." +
                    $"\nPlease make sure you are matching the correct inputs with the correct script." +
                    $"\nIf a script in the chain does not need any input, specify an empty CustomObject input for it.");

                return emptyOutput;
            }

            if (!active)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please set the `{nameof(active)}` input to true to activate the computation.");
                return emptyOutput;
            }

            List<RuntimeMessages> allRuntimeMessages = new List<RuntimeMessages>();
            List<List<RemoteOutputData>> allOutputData = new List<List<RemoteOutputData>>();


            for (int i = 0; i < scriptFilePaths.Count; i++)
            {
                m_PartOfChain = scriptFilePaths.Count > 1;

                Output<RuntimeMessages, List<RemoteOutputData>> scriptResult = new Output<RuntimeMessages, List<RemoteOutputData>>();

                try
                {
                    scriptResult = RunScript(scriptFilePaths.ElementAtOrDefault(i), new List<IObject>() { inputs.ElementAtOrDefault(i) }, gHScriptConfig, active);
                }
                catch
                {
                    m_PartOfChain = false;
                    Log.Clean();
                    Log.RecordError($"Could not compute script `{Path.GetFileName(scriptFilePaths.ElementAtOrDefault(i))}`.");
                }

                if (m_PartOfChain)
                {
                    if (scriptResult?.Item1?.Errors.Any() ?? false)
                        Log.RecordError("Some errors were encountered. Check the individual Log outputs for details.", true);

                    if (scriptResult?.Item1?.Warnings.Any() ?? false)
                        Log.RecordWarning("Some warnings were encountered. Check the individual Log outputs for details.", true);

                    if (scriptResult?.Item1?.Remarks.Any() ?? false)
                        Log.RecordNote("Some Remarks were encountered. Check the individual Log outputs for details.", true);
                }

                allRuntimeMessages.Add(scriptResult.Item1);
                allOutputData.Add(scriptResult.Item2);
            }

            m_PartOfChain = false;

            return new Output<List<RuntimeMessages>, List<List<RemoteOutputData>>>() { Item1 = allRuntimeMessages, Item2 = allOutputData };
        }

        [MultiOutputAttribute(0, "Log", "Log returned by the script.")]
        [MultiOutputAttribute(1, "Outputs", "Outputs returned by the script.")]
        private static Output<RuntimeMessages, List<RemoteOutputData>> RunScript(string scriptFilePath, List<IObject> inputs = null, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            Output<RuntimeMessages, List<RemoteOutputData>> output = new Output<RuntimeMessages, List<RemoteOutputData>>();

            if (!scriptFilePath.IsExistingGhFile())
                return output;

            if (!active)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please set the `{nameof(active)}` input to true to activate the computation.");
                return output;
            }

            var ghDocumentIo = new GH_DocumentIO();
            ghDocumentIo.Open(scriptFilePath);

            GH_Document ghDoc = ghDocumentIo.Document;
            if (ghDoc == null)
            {
                Log.RecordError("Could not extract a Grasshopper definition from the input file.");
                return output;
            }

            GrasshopperDefinition ghDef = ghDoc.ToGrasshopperDefinition(gHScriptConfig);

            // Set inputs
            List<RemoteInputData> allInputsProvided = inputs.ToRemoteInputData();
            List<ResthopperInputTree> allResthopperInputs = allInputsProvided.ToResthopperInputTrees();
            ghDef.SetInputsData(allResthopperInputs);

            // Solve the GrasshopperDefinition.
            ResthopperOutputs outputSchema = ghDef.SolveAndGetOutputs(gHScriptConfig, !m_PartOfChain);

            if (!m_PartOfChain)
                Log.RaiseAllMessagesToUI();

            Log.Clean();

            List<RemoteOutputData> result = outputSchema.ToRemoteOutputDatas(scriptFilePath);

            return new Output<RuntimeMessages, List<RemoteOutputData>>() { Item1 = outputSchema.RuntimeMessages(Path.GetFileName(scriptFilePath)), Item2 = result };
        }
    }
}
