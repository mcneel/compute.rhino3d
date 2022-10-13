using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static object SimplifiedListOfLists(this List<List<object>> tree)
        {
            if (tree == null)
                return null;

            int branchCount = tree.Count;
            if (branchCount == 0)
                return null;

            if (branchCount == 1)
            {
                var onlyBranch = tree.FirstOrDefault();
                if (onlyBranch.Count == 1)
                    return onlyBranch.FirstOrDefault();
                return onlyBranch;
            }

            return tree;
        }
    }
}
