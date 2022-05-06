using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Resthopper.IO;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public ResthopperOutput SolveDefinition()
        {
            HasErrors = false;
            ResthopperOutput result = new ResthopperOutput();

            // solve definition
            GH_Document.Enabled = true;
            GH_Document.NewSolution(false, GH_SolutionMode.CommandLine);

            foreach (string msg in ErrorMessages)
                result.Errors.Add(msg);

            LogRuntimeMessages(GH_Document.ActiveObjects(), result);

            foreach (var kvp in Outputs)
            {
                IGH_Param param = kvp.Value;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new GrasshopperDataTree<ResthopperObject>();
                outputTree.ParamName = kvp.Key;

                IGH_Structure volatileData = param.VolatileData;
                foreach (var path in volatileData.Paths)
                {
                    var resthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in volatileData.get_Branch(path))
                    {
                        if (goo == null)
                            continue;

                        ResthopperObject rhObj = GetResthopperObject(goo);
                        resthopperObjectList.Add(rhObj);
                    }

                    // preserve paths when returning data
                    outputTree.Add(path.ToString(), resthopperObjectList);
                }

                result.Data.Add(outputTree);
            }

            if (result.Data.Count < 1)
                throw new Exception("No output was returned."); // TODO

            return result;
        }
    }
}
