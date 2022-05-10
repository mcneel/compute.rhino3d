using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Newtonsoft.Json;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinitionUtils
    {
        public static void AssignVolatileData<RType, GHType>(IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree, Func<RType, GHType> ghTypeCtor)
        {
            foreach (KeyValuePair<string, List<ResthopperObject>> entry in dataTree)
            {
                GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    ResthopperObject restobj = entry.Value[i];
                    RType rPt = JsonConvert.DeserializeObject<RType>(restobj.Data);
                    GHType data = ghTypeCtor(rPt);
                    gH_Param.AddVolatileData(path, i, data);
                }
            }
        }

    }
}
