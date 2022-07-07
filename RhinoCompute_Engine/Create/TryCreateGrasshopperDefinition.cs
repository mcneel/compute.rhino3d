using System;
using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Create
    {
        public static bool TryCreateGrasshopperDefinition(this ResthopperInput resthopperInput, out GrasshopperDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(resthopperInput.Script))
                throw new Exception("Missing script input.");

            Uri scriptUrl = null;
            if (Uri.TryCreate(resthopperInput.Script, UriKind.Absolute, out scriptUrl))
                definition = Create.GrasshopperDefinition(scriptUrl);

            if (definition == null)
            {
                definition = Create.GrasshopperDefinition(resthopperInput.Script);

                if (definition == null)
                    throw new Exception("Unable to convert Base-64 encoded Grasshopper script to a GrasshopperDefinition object.");
            }

            return true;
        }
    }
}
