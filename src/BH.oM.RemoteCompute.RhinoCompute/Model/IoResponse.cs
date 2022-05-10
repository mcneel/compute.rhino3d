using System.Collections.Generic;

namespace Resthopper.IO
{
    public class IoResponse
    {
        public string Description { get; set; }
        public string CacheKey { get; set; }
        public List<string> InputNames { get; set; }
        public List<string> OutputNames { get; set; }
        public string Icon { get; set; }
        public List<InputParam> Inputs { get; set; }
        public List<IoParam> Outputs { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
