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
        public int[] Path {
            get; set;
        }

        public GhPath()
        {
            //this.Path = new int[0];
        }

        public GhPath(int path) {
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

        public override string ToString()
        {
            string sPath = "{ ";
            foreach(int i in this.Path)
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
        

        public bool LastIndexSame(int i)
        {
            return this.Path.Last() == i;
        }
    }


    public class DataTree<T> 
    {

        public DataTree() {
            tree = new Dictionary<string, List<T>>();
            //_GhPathIndexer = new Dictionary<int, GhPath>();
        }

        private Dictionary<string, List<T>> tree;
        public string ParamName { get; set; }
        //Dictionary<int, GhPath> _GhPathIndexer;


        public Dictionary<string, List<T>> InnerTree {
            get { return tree; }
            set { tree = value; }
        }

        //public string ParamName { get; set; }

/*
        public ICollection<string> Keys {
            get {
                return ((IDictionary<string, List<T>>)tree).Keys;
            }
        }

        public ICollection<List<T>> Values {
            get {
                return ((IDictionary<string, List<T>>)tree).Values;
            }
        }

        public int Count {
            get {
                return ((IDictionary<string, List<T>>)tree).Count;
            }
        }

        public bool IsReadOnly {
            get {
                return ((IDictionary<string, List<T>>)tree).IsReadOnly;
            }
        }
*/
        public List<T> this[string key] {
            get {
                return ((IDictionary<string, List<T>>)tree)[key];
            }

            set {
                ((IDictionary<string, List<T>>)tree)[key] = value;
            }
        }

        public bool Contains(T item) {

            foreach (var list in tree.Values) {
                if (list.Contains(item)) {
                    return true;
                }
            }
            return false;
        }

        public void Append(List<T> items, GhPath GhPath) {
            this.Append(items, GhPath.ToString());            
        }

        public void Append(List<T> items, string GhPath) {

            if (!tree.ContainsKey(GhPath)) {
                tree.Add(GhPath, new List<T>());
            }
            tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public void Append(T item, GhPath path) {
            this.Append(item, path.ToString());
        }

        public void Append(T item, string GhPath) {
            if (!tree.ContainsKey(GhPath)) {
                tree.Add(GhPath, new List<T>());
            }
            tree[GhPath].Add(item);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public bool ContainsKey(string key) {
            return ((IDictionary<string, List<T>>)tree).ContainsKey(key);
        }

        public void Add(string key, List<T> value) {
            ((IDictionary<string, List<T>>)tree).Add(key, value);
        }

        public bool Remove(string key) {
            return ((IDictionary<string, List<T>>)tree).Remove(key);
        }

        public bool TryGetValue(string key, out List<T> value) {
            return ((IDictionary<string, List<T>>)tree).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, List<T>> item) {
            ((IDictionary<string, List<T>>)tree).Add(item);
        }

        public void Clear() {
            ((IDictionary<string, List<T>>)tree).Clear();
        }

        public bool Contains(KeyValuePair<string, List<T>> item) {
            return ((IDictionary<string, List<T>>)tree).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, List<T>>[] array, int arrayIndex) {
            ((IDictionary<string, List<T>>)tree).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, List<T>> item) {
            return ((IDictionary<string, List<T>>)tree).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, List<T>>> GetEnumerator() {
            return ((IDictionary<string, List<T>>)tree).GetEnumerator();
        }

        //IEnumerator IEnumerable.GetEnumerator() {
            //return ((IDictionary<string, List<T>>)tree).GetEnumerator();
        //}
    }

}
