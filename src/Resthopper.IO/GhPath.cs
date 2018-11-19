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
                ints.Add(Int32.Parse(s));
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

    public class ResthopperDataTree : DataTree<ResthopperObject>
    {
        
    }

    //public class DictionaryAsArrayResolver : DefaultContractResolver
    //{
    //    protected override JsonContract CreateContract(Type objectType) {
    //        if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) ||
    //           (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))) {
    //            return base.CreateArrayContract(objectType);
    //        }

    //        return base.CreateContract(objectType);
    //    }
    //}


    public class DataTree<T> : IDictionary<string, List<T>>
    {

        public DataTree() {
            _tree = new Dictionary<string, List<T>>();
            //_GhPathIndexer = new Dictionary<int, GhPath>();
        }

        private Dictionary<string, List<T>> _tree;
        public string ParamName { get; set; }
        //Dictionary<int, GhPath> _GhPathIndexer;


        public Dictionary<string, List<T>> InnerTree {
            get { return _tree; }
            set { _tree = value; }
        }

        //public string ParamName { get; set; }


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

        public void Append(List<T> items, string GhPath) {

            if (!_tree.ContainsKey(GhPath)) {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
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

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IDictionary<string, List<T>>)_tree).GetEnumerator();
        }
    }

}
