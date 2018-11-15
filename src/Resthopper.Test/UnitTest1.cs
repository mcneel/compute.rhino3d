using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Resthopper.IO;
using System.Collections.Generic;
using Rhino.Geometry;
using Newtonsoft.Json;

namespace Resthopper.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void CanSerilaizePath() {
            var path = new GhPath(new int[] { 0, 1, 2 });

            string serialized = JsonConvert.SerializeObject(path);
            GhPath copy = JsonConvert.DeserializeObject<GhPath>(serialized);

            for (int i = 0; i < path.Path.Length; i++) {

                Assert.AreEqual(path.Path[i], copy.Path[i]);                
            }
        }

        [TestMethod]
        public void CanSerializeTree() {

            DataTree<ResthopperObject> tree = new DataTree<ResthopperObject>();

            var path = new GhPath(new int[] { 0, 1, 2 });

            List<ResthopperObject> list = new List<ResthopperObject>() {
                new ResthopperObject("test1"),
                new ResthopperObject("test2"),
                new ResthopperObject("test3"),
            };

            tree.Add(path, list);


            string serialized = JsonConvert.SerializeObject(tree);
            DataTree<ResthopperObject> copy = JsonConvert.DeserializeObject<DataTree<ResthopperObject>>(serialized);


        }



        [TestMethod]
        public void TestMethod1() {
            //datatree of points .. - descr col location (branch -> level)
            //datatree of polylines - descr slabs (branch -> level)

            Schema schema = new Schema();
            
            schema.Algo = "defjskljfe";

            // Generate points
            DataTree<ResthopperObject> tree = new DataTree<ResthopperObject>();
            for (var i = 0; i < 20; i++)
            {
                List<ResthopperObject> colLevel = new List<ResthopperObject>();
                for (var j = 0; j < 10; j++)
                {
                    Point3d pt = new Point3d(0, j, i);
                    colLevel.Add(new ResthopperObject(pt));
                }
                tree.Append(colLevel, new GhPath(new int[] { i } ));
            }
            
            tree.ParamName = "columns";

            ResthopperObject colPoint = new ResthopperObject();

            schema.Values = new System.Collections.Generic.List<DataTree<ResthopperObject>>();
            schema.Values.Add(tree);

            // Serialize
            string serialized = JsonConvert.SerializeObject(schema);

            // Deserialize
            List<List<Point3d>> ExtractedPoints = new List<List<Point3d>>(); 
            Schema sh = JsonConvert.DeserializeObject<Schema>(serialized);
            foreach (DataTree<ResthopperObject> t in sh.Values)
            {
                foreach (KeyValuePair<GhPath, List<ResthopperObject>> entree in t)
                {
                    List<Point3d> LevelPoints = new List<Point3d>();
                    foreach (ResthopperObject obj in entree.Value)
                    {
                        Point3d pt = (Point3d)obj.ExtractData();
                    }
                }
            }
        }
    }
}
