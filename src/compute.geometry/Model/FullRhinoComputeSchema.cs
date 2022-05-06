using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.RemoteCompute.RhinoCompute;
using Newtonsoft.Json;
using BH.oM.RemoteCompute;

namespace Resthopper.IO
{
    public class FullRhinoComputeSchema : RhinoComputeSchema
    {
        public double AbsoluteTolerance { get; set; } = 0;

        public double AngleTolerance { get; set; } = 0;

        public string ModelUnits { get; set; } = Rhino.UnitSystem.Millimeters.ToString();

        // If true on input, the solve results are cached based on this schema.
        // When true the cache is searched for already computed results and used
        public bool CacheSolve { get; set; } = false;

        // Used for nested calls
        public int RecursionLevel { get; set; } = 0;

        public List<string> Warnings { get; set; } = new List<string>();

        public List<string> Errors { get; set; } = new List<string>();
    }
}
