using BH.oM.RemoteCompute.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static double? GetMinimum(this IGH_Param param)
        {
            if (param is IGH_ContextualParameter contextualParameter && param.Sources.Count == 1)
                return GetMinimum(param.Sources[0]);

            if (param is GH_NumberSlider paramSlider)
                return System.Convert.ChangeType(paramSlider.Slider.Minimum, typeof(double)) as double?;

            return null;
        }
    }
}
