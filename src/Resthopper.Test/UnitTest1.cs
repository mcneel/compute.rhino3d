using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Resthopper.IO;
using System.Collections.Generic;

namespace Resthopper.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1() {
            //datatree of points .. - descr col location (branch -> level)
            //datatree of polylines - descr slabs (branch -> level)

            Schema schema = new Schema();
            
            schema.Algo = "defjskljfe";
            
            DataTree<ResthopperObject> tree = new DataTree<ResthopperObject>();
            tree.ParamName = "columns";

            ResthopperObject colPoint = new ResthopperObject();
            colPoint.Data = "Point3d"; //serialized geometry..
            colPoint.Type = GHTypeCodes.gh_GH_IO_3dPoint; //.GetType();

            List<ResthopperObject> colLevel1 = new List<ResthopperObject>() { colPoint };
            List<ResthopperObject> colLevel2 = new List<ResthopperObject>();

            tree.Append(colLevel1, new GhPath(new int[] { 0 }));
            tree.Append(colLevel2, new GhPath(new int[] { 1 }));

            schema.Values = new System.Collections.Generic.List<DataTree<ResthopperObject>>();
        }
    }
}
