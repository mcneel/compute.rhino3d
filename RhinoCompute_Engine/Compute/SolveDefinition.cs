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

        public static ResthopperOutputs SolveDefinition(this GrasshopperDefinition gdef, int recursionLevel = 0)
        {
            ResthopperOutputs resthopperOutput = new ResthopperOutputs();

            bool singleThreaded = recursionLevel == 0; // can't block on recursive calls
            gdef.GH_Document.DefineConstant("ComputeRecursionLevel", new Grasshopper.Kernel.Expressions.GH_Variant(recursionLevel + 1));
            gdef.GH_Document.Enabled = true;

            if (singleThreaded)
                lock (m_ghsolvelock)
                    gdef.GH_Document.NewSolution(false, GH_SolutionMode.Default);
            else
                gdef.GH_Document.NewSolution(false, GH_SolutionMode.Default);

            List<string> errors, warnings, remarks = new List<string>();
            gdef.GH_Document.RuntimeMessages(out errors, out warnings, out remarks);

            foreach (Output output in gdef.Outputs.Values)
            {
                IGH_Param param = output.Param;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new GrasshopperDataTree<ResthopperObject>();
                outputTree.ParamName = output.Name;

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

            // Add the messages from BHoM components
            Log.GetErrors().ForEach(m => errors.Add(m));
            Log.GetWarnings().ForEach(m => warnings.Add(m));
            Log.GetNotes().ForEach(m => remarks.Add(m));

            // Add errors to the resthopperOutput
            errors.ForEach(m => resthopperOutput.Errors.Add(m));
            warnings.ForEach(m => resthopperOutput.Warnings.Add(m));
            remarks.ForEach(m => resthopperOutput.Remarks.Add(m));

            return resthopperOutput;
        }
    }
}
