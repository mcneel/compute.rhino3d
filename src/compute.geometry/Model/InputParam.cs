namespace Resthopper.IO
{
    public class InputParam : IoParam
    {
        public string Description { get; set; }
        public int AtLeast { get; set; } = 1;
        public int AtMost { get; set; } = int.MaxValue;
        public object Default { get; set; } = null;
        public object Minimum { get; set; } = null;
        public object Maximum { get; set; } = null;
    }
}
