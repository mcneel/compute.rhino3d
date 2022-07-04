using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.RemoteCompute.RhinoCompute
{
    // Class that collects all proprerties the original schema found in compute.geometry.
    // Kept for compatibility with existing RhinoCompute scripts.
    public class FormerSchema
    {
        public double AbsoluteTolerance { get; set; } = 0;

        public double AngleTolerance { get; set; } = 0;

        public string ModelUnits { get; set; } = Rhino.UnitSystem.Millimeters.ToString();

        public string Algo { get; set; }

        public string Pointer { get; set; }

        public bool CacheSolve { get; set; } = false;

        public int RecursionLevel { get; set; } = 0;

        public List<GrasshopperDataTree<ResthopperObject>> Values { get; set; } = new List<GrasshopperDataTree<ResthopperObject>>();

        public List<string> Warnings { get; set; } = new List<string>();

        public List<string> Errors { get; set; } = new List<string>();
    }
}
