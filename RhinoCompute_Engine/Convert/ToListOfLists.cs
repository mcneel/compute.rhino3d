using System;
using System.Collections;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Convert
    {
        public static List<List<object>> ToListOfLists<G>(this GH_PersistentParam<G> persistentParam) where G : class, IGH_Goo
        {
            Grasshopper.Kernel.Data.GH_Structure<G> ghstructure = persistentParam.PersistentData;

            return ghstructure.ToListOfLists();
        }

        public static List<List<object>> ToListOfLists<G>(this Grasshopper.Kernel.Data.GH_Structure<G> ghstructure) where G : class, IGH_Goo
        {
            List<List<object>> result = new List<List<object>>();

            foreach (var branch in ghstructure.Branches)
            {
                List<object> branchList = new List<object>();

                IEnumerable branchIEnumerable = branch as IEnumerable;
                if (branchIEnumerable == null)
                    try
                    {
                        // Grasshopper sucks. How can they not know how to use interfaces? Incredible.
                        branchList.Add((branch as dynamic).Value);
                    }
                    catch { }

                if (branchIEnumerable != null)
                    foreach (var element in branchIEnumerable)
                    {
                        try
                        {
                            // Grasshopper sucks. How can they not know how to use interfaces? Incredible.
                            branchList.Add((element as dynamic).Value);
                        }
                        catch { }
                    }

                result.Add(branchList);
            }

            return result;
        }
    }
}
