using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute.RhinoCompute.Schemas;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static FormerRestSchema FromBHoM(this ResthopperOutputs output)
        {
            if (output == null)
                return default(FormerRestSchema);

            FormerRestSchema result = new FormerRestSchema()
            {
                Values = output.OutputsData,
                Errors = output.Errors,
                Warnings = output.Warnings
            };

            return result;
        }
    }
}
