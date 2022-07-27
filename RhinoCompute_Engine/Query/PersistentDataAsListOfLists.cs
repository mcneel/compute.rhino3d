using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
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
