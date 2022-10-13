using BH.oM.Computing.RhinoCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;


namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        public static double? GetMaximum(this IGH_Param param)
        {
            if (param == null)
                return null;

            if (param.Sources.Count == 1)
                return GetMaximum(param.Sources[0]);

            if (param is GH_NumberSlider paramSlider)
                return System.Convert.ChangeType(paramSlider.Slider.Maximum, typeof(double)) as double?;

            return null;
        }
    }
}
