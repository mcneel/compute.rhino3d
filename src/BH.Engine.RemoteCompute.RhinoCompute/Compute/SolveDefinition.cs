using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using BH.oM.RemoteCompute;
using BH.Engine.RemoteCompute.RhinoCompute;
using BH.Engine.RemoteCompute.RhinoCompute.Objects;

namespace compute.geometry
{
    public static partial class Compute
    {
        public static ResthopperOutput SolveDefinition(this GrasshopperDefinition gdef)
        {
            ResthopperOutput resthopperOutput = new ResthopperOutput();

            // solve definition
            gdef.GH_Document.Enabled = true;
            gdef.GH_Document.NewSolution(false, GH_SolutionMode.CommandLine);

            List<string> errors, warnings, remarks = new List<string>();
            gdef.GH_Document.RuntimeMessages(out errors, out warnings, out remarks);

            errors.ForEach(m => gdef.Errors.Add(m));
            warnings.ForEach(m => gdef.Warnings.Add(m));
            remarks.ForEach(m => gdef.Remarks.Add(m));

            errors.ForEach(m => resthopperOutput.Errors.Add(m));
            warnings.ForEach(m => resthopperOutput.Warnings.Add(m));
            remarks.ForEach(m => resthopperOutput.Remarks.Add(m));

            foreach (var kv in gdef.Outputs)
            {
                IGH_Param param = kv.Value;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new GrasshopperDataTree<ResthopperObject>();
                outputTree.ParamName = kv.Key;

                IGH_Structure volatileData = param.VolatileData;
                foreach (var path in volatileData.Paths)
                {
                    var resthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in volatileData.get_Branch(path))
                    {
                        if (goo == null)
                            continue;

                        ResthopperObject rhObj = BH.Engine.RemoteCompute.RhinoCompute.Convert.ToResthopperObject(goo);
                        resthopperObjectList.Add(rhObj);
                    }

                    // preserve paths when returning data
                    outputTree.Add(path.ToString(), resthopperObjectList);
                }

                resthopperOutput.Data.Add(outputTree);
            }

            if (resthopperOutput.Data.Count < 1)
                gdef.Errors.Add("No output was returned.");

            return resthopperOutput;
        }
    }
}
