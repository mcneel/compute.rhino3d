using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static IEnumerable<GH_PersistentParam<IGH_Goo>> PersistentData(this IGH_Param persistentParam)
        {
            try
            {
                // Grasshopper sucks. How can they not know how to use interfaces? Incredible.
                dynamic persistentData = (persistentParam as dynamic).PersistentData;
                return persistentData as IEnumerable<GH_PersistentParam<IGH_Goo>>;
            }
            catch { }

            return null;
        }

        public static IEnumerable<IGH_Goo> VolatileData(this IGH_Param persistentParam)
        {
            try
            {
                // Grasshopper sucks. How can they not know how to use interfaces? Incredible.
                dynamic persistentData = (persistentParam as dynamic).VolatileData;
                return persistentData as IEnumerable<IGH_Goo>;
            }
            catch { }

            return null;
        }

        public static List<List<object>> PersistentDataAsListOfLists(this IGH_Param persistentParam)
        {
            try
            {
                // Grasshopper sucks. How can they not know how to use interfaces? Incredible.
                dynamic persistentData = (persistentParam as dynamic).PersistentData;
                return Convert.ToListOfLists(persistentData) as List<List<object>>;
            }
            catch { }

            return new List<List<object>>();
        }
    }
}
