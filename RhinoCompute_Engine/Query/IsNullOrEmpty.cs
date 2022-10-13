using System.Collections.Generic;
using System.Linq;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing;
using BH.oM.Computing.RhinoCompute.Schemas;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static bool IsNullOrEmpty(this ResthopperInputs obj)
        {
            if (obj == null)
                return true;

            if (obj.InputsData.IsNullOrEmpty())
            {
                if (obj is Base64ScriptInput base64Script && string.IsNullOrWhiteSpace(base64Script.Base64Script))
                    return true;
                if (obj is ScriptUrlInput urlInput && string.IsNullOrWhiteSpace(urlInput.ScriptUrl.ToString()))
                    return true;
                if (obj is CacheKeyInput cacheKeyInput && string.IsNullOrWhiteSpace(cacheKeyInput.CacheKey))
                    return true;
            }

            return false;
        }
    }
}
