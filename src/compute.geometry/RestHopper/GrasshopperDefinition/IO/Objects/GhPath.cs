using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Resthopper.IO
{
    public class GhPath
    {
        public int[] Path { get; set; }

        public GhPath()
        {
            //this.Path = new int[0];
        }

        public GhPath(int path)
        {
            this.Path = new int[] { path };
        }


        public GhPath(int[] path)
        {
            this.Path = path;
        }

        public GhPath(string path)
        {
            this.Path = FromString(path);
        }

        public GhPath(GhPath pathObj, int i)
        {
            int[] path = pathObj.Path;
            this.Path = new int[path.Length + 1];

            for (int j = 0; j < path.Length; j++)
            {
                this.Path[j] = path[j];
            }
            this.Path[path.Length] = i;
        }

        public override string ToString()
        {
            string sPath = "{ ";
            foreach (int i in this.Path)
            {
                sPath += $"{i}; ";
            }
            sPath += "}";
            return sPath;
        }

        public static int[] FromString(string path)
        {
            string primer = path.Replace(" ", "").Replace("{", "").Replace("}", "");
            string[] stringValues = primer.Split(';');
            List<int> ints = new List<int>();

            foreach (string s in stringValues)
            {
                if (s != string.Empty)
                {
                    ints.Add(Int32.Parse(s));
                }
            }

            return ints.ToArray();
        }

        public bool LastIndexSame(int i)
        {
            return this.Path.Last() == i;
        }
    }

}
