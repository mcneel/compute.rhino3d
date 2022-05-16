using BH.oM.RemoteCompute;
using BH.oM.RemoteCompute.RhinoCompute;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static FormerSchema FromBHoM(this ResthopperOutput output)
        {
            if (output == null)
                return default(FormerSchema);

            FormerSchema result = new FormerSchema()
            {
                Values = output.Data,
                Errors = output.Errors,
                Warnings = output.Warnings
            };

            return result;
        }
    }
}
