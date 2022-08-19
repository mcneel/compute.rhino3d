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
using System.Collections;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Compute
    {
        private static bool m_PartOfChain = false;
        private static bool m_repeatedExecutionMultiInputs = false;
        private static bool m_askToReenable = true;

        [Input("scriptFilePaths", "Scripts to be run. They will be run in the order provided, independently from each other.")]
        [Input("inputs", "The full list of script will be executed for every input in this list.")]
        public static List<ComputationOutput> RunScriptChain(List<string> scriptFilePaths, List<CustomObject> inputs = null, bool chainIO = true, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            var emptyOutput = new List<ComputationOutput>();

            if (!active)
            {
                BH.Engine.Base.Compute.RecordWarning($"Please set the `{nameof(active)}` input to true to activate the computation.");
                return emptyOutput;
            }

            List<ComputationOutput> allOutputs = new List<ComputationOutput>();

            m_PartOfChain = scriptFilePaths.Count > 1;
            m_repeatedExecutionMultiInputs = inputs.Count > 1;

            // Add empty CustomObject to the inputs to allow computation.
            if (inputs == null || !inputs.Any())
                inputs.Add(new CustomObject()); 

            if (inputs.Count <= 1 && scriptFilePaths.Count <= 1)
                m_askToReenable = false;

            if (m_askToReenable && inputs.Count > 1)
            {
                BH.Engine.Base.Compute.RecordWarning($"You provided {inputs.Count} inputs, which means that each script will be run {inputs.Count} times." +
                    $"\nPlease be aware that this may take some time." +
                    $"\nRe-enable this component to continue.");
                m_askToReenable = false;
                return emptyOutput;
            }

            // Each script is to be run once for each input.
            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];

                ComputationOutput scriptResult = new ComputationOutput();

                for (int j = 0; j < scriptFilePaths.Count; j++)
                {
                    try
                    {
                        if (chainIO && j != 0)
                        {
                            input = new CustomObject();

                            foreach (var g in scriptResult.OutputDatas.GroupBy(r => r.Name))
                                if (input.CustomData.ContainsKey(g.Key))
                                    input.CustomData[g.Key] = g.ToList().ToRemoteInputData();
                        }

                        scriptResult = RunScript(scriptFilePaths.ElementAtOrDefault(j), input, gHScriptConfig, active);
                    }
                    catch
                    {
                        m_PartOfChain = j != 0;
                        Log.Clean();
                        Log.RecordError($"Could not compute script `{Path.GetFileName(scriptFilePaths.ElementAtOrDefault(j))}`.");
                    }

                    allOutputs.Add(scriptResult);
                }

            }

            if (m_PartOfChain || m_repeatedExecutionMultiInputs)
                allOutputs.ReportClusteredMessagesToUI();

            m_PartOfChain = false;
            m_askToReenable = true;

            return allOutputs;
        }

        [Input("scriptFilePaths", "Scripts to be run. They will be run in the order provided, independently from each other.")]
        [Input("inputs", "Inputs for the scripts. The number of inputs provided must match the number of scripts provided. If a script does not need an input, provide an empty CustomObject for it.")]
        public static List<ComputationOutput> RunScripts(List<string> scriptFilePaths, List<CustomObject> inputs = null, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            var emptyOutput = new List<ComputationOutput>();

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

            List<ComputationOutput> allOutputs = new List<ComputationOutput>();

            m_PartOfChain = scriptFilePaths.Count > 1;
            m_repeatedExecutionMultiInputs = inputs.Count > 1;

            for (int i = 0; i < scriptFilePaths.Count; i++)
            {
                m_PartOfChain = scriptFilePaths.Count > 1;

                var scriptResult = new ComputationOutput();

                try
                {
                    scriptResult = RunScript(scriptFilePaths.ElementAtOrDefault(i), inputs.ElementAtOrDefault(i), gHScriptConfig, active);
                }
                catch
                {
                    m_PartOfChain = i != 0;
                    Log.Clean();
                    BH.Engine.Base.Compute.RecordError($"Could not compute script `{Path.GetFileName(scriptFilePaths.ElementAtOrDefault(i))}`.");
                }

                allOutputs.Add(scriptResult);
            }

            if (m_PartOfChain || m_repeatedExecutionMultiInputs)
                allOutputs.ReportClusteredMessagesToUI();

            m_PartOfChain = false;

            return allOutputs;
        }

        private static ComputationOutput RunScript(string scriptFilePath, CustomObject inputs = null, GHScriptConfig gHScriptConfig = null, bool active = false)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            var output = new ComputationOutput();

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
            List<RemoteInputData<object>> allInputsProvided = inputs.ToRemoteInputData();
            List<ResthopperInputTree> allResthopperInputs = allInputsProvided.ToResthopperInputTrees();
            ghDef.SetInputsData(allResthopperInputs);

            // Solve the GrasshopperDefinition.
            ResthopperOutputs outputSchema = ghDef.SolveAndGetOutputs(gHScriptConfig, (!m_PartOfChain && !m_repeatedExecutionMultiInputs));

            if (!m_PartOfChain && !m_repeatedExecutionMultiInputs)
                Log.RaiseAllMessagesToUI();
            else
            {
                outputSchema.Errors.AddRange(Log.GetErrors());
                outputSchema.Warnings.AddRange(Log.GetWarnings());
                outputSchema.Remarks.AddRange(Log.GetNotes());
            }

            Log.Clean();

            var result = outputSchema.ToRemoteOutputDatas(scriptFilePath);

            result.Inputs = inputs.ToRemoteInputData();

            return result;
        }

        private static void ReportClusteredMessagesToUI(this List<ComputationOutput> allOutputs)
        {
            if (allOutputs.Select(o => o.Log)?.Any(rm => rm?.Errors.Any() ?? false) ?? false)
                BH.Engine.Base.Compute.RecordError($"Some Errors were encountered in these scripts:\n     `{string.Join("`,\n     ", allOutputs?.Where(o => o.Log.Errors.Any()).Select(o => o.SourceScript).Distinct())}`." +
                    $"\nCheck the individual Logs output for details.");

            if (allOutputs.Select(o => o.Log)?.Any(rm => rm?.Warnings.Any() ?? false) ?? false)
                BH.Engine.Base.Compute.RecordWarning($"Some Warnings were encountered in these scripts:\n     `{string.Join("`,\n     ", allOutputs?.Where(o => o.Log.Warnings.Any()).Select(o => o.SourceScript).Distinct())}`." +
                    $"\nCheck the individual Logs output for details.");

            if (allOutputs.Select(o => o.Log)?.Any(rm => rm?.Remarks.Any() ?? false) ?? false)
                BH.Engine.Base.Compute.RecordNote($"Some Remarks were encountered in these scripts:\n     `{string.Join("`,\n     `", allOutputs?.Where(o => o.Log.Remarks.Any()).Select(o => o.SourceScript).Distinct())}`." +
                    $"\nCheck the individual Logs output for details.");
        }
    }
}
