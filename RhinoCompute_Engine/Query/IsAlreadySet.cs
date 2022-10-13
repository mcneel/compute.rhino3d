using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsAlreadySet(this Input inputGroup, GrasshopperDataTree<ResthopperObject> tree)
        {
            if (inputGroup.InputData == null)
                return false;

            var oldDictionary = inputGroup.InputData.InnerTree;
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
