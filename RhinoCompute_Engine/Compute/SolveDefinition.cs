using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using BH.oM.Computing;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;
using Log = BH.Engine.Computing.Log;
using System;
using System.Linq;
using Grasshopper;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Compute
    {
        private static object m_ghsolvelock = new object();

        public static void SolveDefinition(this GrasshopperDefinition ghDef, GHScriptConfig gHScriptConfig = null)
        {
            if (gHScriptConfig == null)
                gHScriptConfig = new GHScriptConfig();

            bool singleThreaded = gHScriptConfig.RecursionLevel == 0; // can't block on recursive calls
            ghDef.GH_Document.DefineConstant("ComputeRecursionLevel", new Grasshopper.Kernel.Expressions.GH_Variant(gHScriptConfig.RecursionLevel + 1));
            ghDef.GH_Document.Enabled = true;

            ghDef.SetTriggers();

            if (singleThreaded)
                lock (m_ghsolvelock)
                    ghDef.GH_Document.NewSolution(false, GH_SolutionMode.Default);
            else
                ghDef.GH_Document.NewSolution(false, GH_SolutionMode.Default);

            ghDef.IsSolved = true;
        }
    }
}
