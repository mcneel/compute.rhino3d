using System.Linq;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using System.Collections.Generic;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;
using System;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static ResthopperInputs ToBHoM(this FormerRestSchema formerSchema)
        {
            if (formerSchema == null)
                return default(ResthopperInputs);

            IEnumerable<ResthopperInputTree> inputData = formerSchema.Values.Select(v => new ResthopperInputTree() { InnerTree = v.InnerTree, ParamName = v.ParamName });


            if (string.IsNullOrWhiteSpace(formerSchema.Pointer))
            {
                return new Base64ScriptInput() { Base64Script = formerSchema.Algo, InputsData = inputData, RecursionLevel = formerSchema.RecursionLevel, CacheToDisk = formerSchema.CacheSolve };
            }
            else
            {
                if (!Uri.TryCreate(formerSchema.Pointer, UriKind.Absolute, out Uri uri))
                {
                    Log.RecordError("Could create Uri from Former schema Uri.");
                    return null;
                }

                return new ScriptUrlInput() { ScriptUrl = uri, InputsData = inputData, RecursionLevel = formerSchema.RecursionLevel, CacheToMemory = formerSchema.CacheSolve };
            }
        }
    }
}
