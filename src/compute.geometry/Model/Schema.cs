using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Resthopper.IO
{
    public class Schema
    {
        public Schema() { }

        [JsonProperty(PropertyName = "absolutetolerance")]
        public double AbsoluteTolerance { get; set; } = 0;

        [JsonProperty(PropertyName = "angletolerance")]
        public double AngleTolerance { get; set; } = 0;

        [JsonProperty(PropertyName = "modelunits")]
        public string ModelUnits { get; set; } = Rhino.UnitSystem.Millimeters.ToString();

        /// <summary>
        ///  Can be used to store a Base-64 encoded GH script. Heaby; prefer Pointer instead.
        /// </summary>
        [JsonProperty(PropertyName = "algo")]
        public string Algo { get; set; }

        /// <summary>
        ///  Can be used to store an URL of a GH script hosted elsewhere. The script will be downloaded, cached and then executed by RestHopper upon request.
        /// </summary>
        [JsonProperty(PropertyName = "pointer")]
        public string Pointer { get; set; }

        // If true on input, the solve results are cached based on this schema.
        // When true the cache is searched for already computed results and used
        [JsonProperty(PropertyName = "cachesolve")]
        public bool CacheSolve { get; set; } = false;

        // Used for nested calls
        [JsonProperty(PropertyName = "recursionlevel")]
        public int RecursionLevel { get; set; } = 0;

        [JsonProperty(PropertyName = "values")]
        public List<DataTree<ResthopperObject>> Values { get; set; } = new List<DataTree<ResthopperObject>>();

        // Return warnings from GH
        [JsonProperty(PropertyName = "warnings")]
        public List<string> Warnings { get; set; } = new List<string>();

        // Return errors from GH
        [JsonProperty(PropertyName = "errors")]
        public List<string> Errors { get; set; } = new List<string>();
    }
}
