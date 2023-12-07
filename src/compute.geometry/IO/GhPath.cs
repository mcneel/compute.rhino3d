using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rhino.Geometry;

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

    public class GrasshopperValues
    {
        [JsonIgnore]
        public readonly Dictionary<string, Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>> Values = new Dictionary<string, Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>>();
        public string Data 
        {
            get
            {
                var archive = new GH_Archive();
                archive.CreateNewRoot(true);
                var chunk = archive.GetRootNode;

                foreach (var entry in Values)
                {
                    var param = chunk.CreateChunk(entry.Key);

                    foreach (var list in entry.Value.Branches)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var goo = list[i];
                            // Removing ref ID in order to send as internalized geometry
                            if (goo is IGH_GeometricGoo geometricGoo && geometricGoo.IsReferencedGeometry)
                            {
                                geometricGoo = geometricGoo.DuplicateGeometry();
                                geometricGoo.ReferenceID = Guid.Empty;
                                list[i] = geometricGoo;
                            }
                        }
                    }

                    entry.Value.Write(param);
                }

                var binary = archive.Serialize_Binary();
                return Convert.ToBase64String(binary);
            }
            set
            {
                Values.Clear();
                var base64 = value;
                var binary = Convert.FromBase64String(base64);
                var archive = new GH_Archive();
                archive.Deserialize_Binary(binary);
                var chunk = archive.GetRootNode;
                foreach (var param in chunk.Chunks)
                {
                    var values = new Grasshopper.Kernel.Data.GH_Structure<IGH_Goo>();
                    values.Read(param as GH_IReader);
                    Values.Add(param.Name, values);
                }
            }
        }
    }

    public class DataTree<T>
    {
        public DataTree() {
            _tree = new Dictionary<string, List<T>>();
            //_GhPathIndexer = new Dictionary<int, GhPath>();
        }

        public string ParamName { get; set; }

        private Dictionary<string, List<T>> _tree;

        //Dictionary<int, GhPath> _GhPathIndexer;


        public Dictionary<string, List<T>> InnerTree {
            get { return _tree; }
            set { _tree = value; }
        }

        //public string ParamName { get; set; }

/*
        public ICollection<string> Keys {
            get {
                return ((IDictionary<string, List<T>>)_tree).Keys;
            }
        }

        public ICollection<List<T>> Values {
            get {
                return ((IDictionary<string, List<T>>)_tree).Values;
            }
        }

        public int Count {
            get {
                return ((IDictionary<string, List<T>>)_tree).Count;
            }
        }

        public bool IsReadOnly {
            get {
                return ((IDictionary<string, List<T>>)_tree).IsReadOnly;
            }
        }
*/
        public List<T> this[string key] {
            get {
                return ((IDictionary<string, List<T>>)_tree)[key];
            }

            set {
                ((IDictionary<string, List<T>>)_tree)[key] = value;
            }
        }

        public bool Contains(T item) {

            foreach (var list in _tree.Values) {
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

            if (!_tree.ContainsKey(GhPath)) {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public void Append(T item, GhPath path) {
            this.Append(item, path.ToString());
        }

        public void Append(T item, string GhPath) {
            if (!_tree.ContainsKey(GhPath)) {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].Add(item);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public bool ContainsKey(string key) {
            return ((IDictionary<string, List<T>>)_tree).ContainsKey(key);
        }

        public void Add(string key, List<T> value) {
            ((IDictionary<string, List<T>>)_tree).Add(key, value);
        }

        public bool Remove(string key) {
            return ((IDictionary<string, List<T>>)_tree).Remove(key);
        }

        public bool TryGetValue(string key, out List<T> value) {
            return ((IDictionary<string, List<T>>)_tree).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, List<T>> item) {
            ((IDictionary<string, List<T>>)_tree).Add(item);
        }

        public void Clear() {
            ((IDictionary<string, List<T>>)_tree).Clear();
        }

        public bool Contains(KeyValuePair<string, List<T>> item) {
            return ((IDictionary<string, List<T>>)_tree).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, List<T>>[] array, int arrayIndex) {
            ((IDictionary<string, List<T>>)_tree).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, List<T>> item) {
            return ((IDictionary<string, List<T>>)_tree).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, List<T>>> GetEnumerator() {
            return ((IDictionary<string, List<T>>)_tree).GetEnumerator();
        }

        //IEnumerator IEnumerable.GetEnumerator() {
            //return ((IDictionary<string, List<T>>)_tree).GetEnumerator();
        //}
    }

}
