using BH.oM.Computing;
using BH.oM.Computing.RhinoCompute;
using BH.oM.Computing.RhinoCompute.Schemas;

namespace BH.Engine.Computing.RhinoCompute
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
