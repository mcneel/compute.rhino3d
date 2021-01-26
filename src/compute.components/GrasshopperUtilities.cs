// The plan is to eventually share this source file between compute.geometry and compute.components
using System;
using GH_IO.Serialization;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace compute.shared
{
    static class GrasshopperUtilities
    {
        public static void GetIOParams(this GH_Document definition, out Dictionary<string, IGH_Param> inputParams, out Dictionary<string, IGH_Param> outputParams)
        {
            inputParams = new Dictionary<string, IGH_Param>();
            outputParams = new Dictionary<string, IGH_Param>();

            foreach (var obj in definition.Objects)
            {
                var group = obj as Grasshopper.Kernel.Special.GH_Group;
                if (group == null)
                    continue;

                string nickname = group.NickName;
                var groupObjects = group.Objects();
                if (nickname.StartsWith("RH_IN") && groupObjects.Count > 0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null)
                    {
                        nickname = nickname.Substring("RH_IN".Length).TrimStart(new char[] { ':', ' ' });
                        inputParams[nickname] = param;
                    }
                }

                if (nickname.StartsWith("RH_OUT") && groupObjects.Count > 0)
                {
                    var param = groupObjects[0] as IGH_Param;
                    if (param != null)
                    {
                        nickname = nickname.Substring("RH_OUT".Length).TrimStart(new char[] { ':', ' ' });
                        outputParams[nickname] = param;
                    }
                }
            }
        }
    }
}
