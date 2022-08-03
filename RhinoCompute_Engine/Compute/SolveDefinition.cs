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
        private static object m_ghsolvelock = new object();

        public static ResthopperOutputs ResthopperOutputs(this GrasshopperDefinition ghDef)
        {
            ResthopperOutputs resthopperOutput = new ResthopperOutputs();

            if (!ghDef.IsSolved)
            {
                Log.RecordError($"Definition not yet solved. Invoke {nameof(SolveDefinition)} on it first.");
                return resthopperOutput;
            }

            foreach (Output output in ghDef.Outputs.Values)
            {
                IGH_Param param = output.Param;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new GrasshopperDataTree<ResthopperObject>();
                outputTree.ParamName = output.Name;
                outputTree.Description = output.Description;

                IGH_Structure volatileData = param.VolatileData;
                foreach (var path in volatileData.Paths)
                {
                    List<ResthopperObject> resthopperObjectList = new List<ResthopperObject>();
                    System.Collections.IList goos = volatileData.get_Branch(path);
                    foreach (object goo in goos)
                    {
                        if (goo == null)
                            continue;

                        ResthopperObject rhObj = BH.Engine.RemoteCompute.RhinoCompute.Convert.ToResthopperObject(goo);
                        resthopperObjectList.Add(rhObj);
                    }

                    // preserve paths when returning data
                    outputTree.Add(path.ToString(), resthopperObjectList);
                }

                resthopperOutput.OutputsData.Add(outputTree);
            }

            if (resthopperOutput.OutputsData.Count < 1)
                Log.RecordNote("No output was returned.");

            // Add messages to the resthopperOutput
            var runtimeMessages = ghDef.GH_Document.RuntimeMessages();
            runtimeMessages.Errors.ForEach(m => resthopperOutput.Errors.Add(m));
            runtimeMessages.Warnings.ForEach(m => resthopperOutput.Warnings.Add(m));
            runtimeMessages.Remarks.ForEach(m => resthopperOutput.Remarks.Add(m));

            return resthopperOutput;
        }

        public static void SolveDefinition(this GrasshopperDefinition ghDef, int recursionLevel = 0, bool raiseMessages = true)
        {
            bool singleThreaded = recursionLevel == 0; // can't block on recursive calls
            ghDef.GH_Document.DefineConstant("ComputeRecursionLevel", new Grasshopper.Kernel.Expressions.GH_Variant(recursionLevel + 1));
            ghDef.GH_Document.Enabled = true;

            if (singleThreaded)
                lock (m_ghsolvelock)
                    ghDef.GH_Document.NewSolution(false, GH_SolutionMode.Default);
            else
                ghDef.GH_Document.NewSolution(false, GH_SolutionMode.Default);

            ghDef.IsSolved = true;

            if (raiseMessages)
            {
                var runtimeMessages = ghDef.GH_Document.RuntimeMessages();
                runtimeMessages.Errors.ForEach(m => Log.RecordError(m));
                runtimeMessages.Warnings.ForEach(m => Log.RecordWarning(m));
                runtimeMessages.Remarks.ForEach(m => Log.RecordNote(m));
            }
        }
    }
}
