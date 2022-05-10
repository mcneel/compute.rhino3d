using System.Collections.Generic;
using System.Linq;
using BH.Engine.RemoteCompute.RhinoCompute.Objects;
using BH.oM.RemoteCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using BH.oM.RemoteCompute.RhinoCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsAlreadySet(this InputGroup inputGroup, GrasshopperDataTree<ResthopperObject> tree)
        {
            if (inputGroup.DataTree == null)
                return false;

            var oldDictionary = inputGroup.DataTree.InnerTree;
            var newDictionary = tree.InnerTree;

            if (!oldDictionary.Keys.SequenceEqual(newDictionary.Keys))
                return false;

            foreach (var kvp in oldDictionary)
            {
                var oldValue = kvp.Value;
                if (!newDictionary.TryGetValue(kvp.Key, out List<ResthopperObject> newValue))
                    return false;

                if (!newValue.SequenceEqual(oldValue))
                    return false;
            }

            return true;
        }
    }
}
