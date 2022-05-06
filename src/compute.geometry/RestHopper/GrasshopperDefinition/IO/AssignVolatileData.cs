using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public void AssignVolatileData<RType, GHType>(IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree, Func<RType, GHType> ghTypeCtor)
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
