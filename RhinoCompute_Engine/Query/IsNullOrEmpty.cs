using System.Collections.Generic;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;

namespace BH.Engine.RemoteCompute.RhinoCompute
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
