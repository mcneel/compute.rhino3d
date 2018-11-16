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

    public class DictionaryAsArrayResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType) {
            if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) ||
               (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))) {
                return base.CreateArrayContract(objectType);
            }

            return base.CreateContract(objectType);
        }
    }


    public class DataTree<T> : IDictionary<GhPath, List<T>>
    {

        public DataTree() {
            _tree = new Dictionary<GhPath, List<T>>();
            //_GhPathIndexer = new Dictionary<int, GhPath>();
        }

        private Dictionary<GhPath, List<T>> _tree;
        //Dictionary<int, GhPath> _GhPathIndexer;


        public Dictionary<GhPath, List<T>> InnerTree {
            get { return _tree; }
            set { _tree = value; }
        }

        //public string ParamName { get; set; }


        public ICollection<GhPath> Keys {
            get {
                return ((IDictionary<GhPath, List<T>>)_tree).Keys;
            }
        }

        public ICollection<List<T>> Values {
            get {
                return ((IDictionary<GhPath, List<T>>)_tree).Values;
            }
        }

        public int Count {
            get {
                return ((IDictionary<GhPath, List<T>>)_tree).Count;
            }
        }

        public bool IsReadOnly {
            get {
                return ((IDictionary<GhPath, List<T>>)_tree).IsReadOnly;
            }
        }

        public List<T> this[GhPath key] {
            get {
                return ((IDictionary<GhPath, List<T>>)_tree)[key];
            }

            set {
                ((IDictionary<GhPath, List<T>>)_tree)[key] = value;
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

            if (!_tree.ContainsKey(GhPath)) {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public void Append(T item, GhPath GhPath) {
            if (!_tree.ContainsKey(GhPath)) {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].Add(item);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public bool ContainsKey(GhPath key) {
            return ((IDictionary<GhPath, List<T>>)_tree).ContainsKey(key);
        }

        public void Add(GhPath key, List<T> value) {
            ((IDictionary<GhPath, List<T>>)_tree).Add(key, value);
        }

        public bool Remove(GhPath key) {
            return ((IDictionary<GhPath, List<T>>)_tree).Remove(key);
        }

        public bool TryGetValue(GhPath key, out List<T> value) {
            return ((IDictionary<GhPath, List<T>>)_tree).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<GhPath, List<T>> item) {
            ((IDictionary<GhPath, List<T>>)_tree).Add(item);
        }

        public void Clear() {
            ((IDictionary<GhPath, List<T>>)_tree).Clear();
        }

        public bool Contains(KeyValuePair<GhPath, List<T>> item) {
            return ((IDictionary<GhPath, List<T>>)_tree).Contains(item);
        }

        public void CopyTo(KeyValuePair<GhPath, List<T>>[] array, int arrayIndex) {
            ((IDictionary<GhPath, List<T>>)_tree).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<GhPath, List<T>> item) {
            return ((IDictionary<GhPath, List<T>>)_tree).Remove(item);
        }

        public IEnumerator<KeyValuePair<GhPath, List<T>>> GetEnumerator() {
            return ((IDictionary<GhPath, List<T>>)_tree).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IDictionary<GhPath, List<T>>)_tree).GetEnumerator();
        }
    }

}
