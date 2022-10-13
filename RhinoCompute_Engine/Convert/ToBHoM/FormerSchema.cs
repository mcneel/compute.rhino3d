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

            GHScriptConfig gHScriptConfig = new GHScriptConfig() { RecursionLevel = formerSchema.RecursionLevel };
            if (string.IsNullOrWhiteSpace(formerSchema.Pointer))
            {
                return new Base64ScriptInput(formerSchema.Algo, gHScriptConfig) { InputsData = inputData, CacheToDisk = formerSchema.CacheSolve };
            }
            else
            {
                if (!Uri.TryCreate(formerSchema.Pointer, UriKind.Absolute, out Uri uri))
                {
                    Log.RecordError("Could create Uri from Former schema Uri.");
                    return null;
                }

                return new ScriptUrlInput(uri, gHScriptConfig) { InputsData = inputData, CacheToMemory = formerSchema.CacheSolve };
            }
        }
    }
}
