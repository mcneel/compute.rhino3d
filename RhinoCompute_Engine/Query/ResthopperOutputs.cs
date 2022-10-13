using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static ResthopperOutputs ResthopperOutputs(this GrasshopperDefinition ghDef)
        {
            ResthopperOutputs resthopperOutput = new ResthopperOutputs();

            if (!ghDef.IsSolved)
            {
                Log.RecordError($"{nameof(GrasshopperDefinition)} not yet solved. Invoke {nameof(Compute)}.{nameof(Compute.SolveDefinition)} on it first.");
                return resthopperOutput;
            }

            foreach (Output output in ghDef.Outputs.Values)
            {
                IGH_Param param = output.Data;
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
    }
}
