using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static double? GetMinimum(this IGH_Param param)
        {
            if (param == null)
                return null;

            if (param.Sources.Count == 1 && param.Sources[0] is GH_NumberSlider)
                return GetMinimum(param.Sources[0]);

            if (param is GH_NumberSlider paramSlider)
                return System.Convert.ChangeType(paramSlider.Slider.Minimum, typeof(double)) as double?;

            return null;
        }
    }
}
