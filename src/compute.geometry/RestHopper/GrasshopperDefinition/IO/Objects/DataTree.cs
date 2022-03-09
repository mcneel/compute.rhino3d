using System.Collections.Generic;

namespace Resthopper.IO
{
    public class DataTree<T>
    {
        public DataTree()
        {
            _tree = new Dictionary<string, List<T>>();
            //_GhPathIndexer = new Dictionary<int, GhPath>();
        }

        private Dictionary<string, List<T>> _tree;
        public string ParamName { get; set; }

        //Dictionary<int, GhPath> _GhPathIndexer;

        public Dictionary<string, List<T>> InnerTree
        {
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

        public List<T> this[string key]
        {
            get
            {
                return ((IDictionary<string, List<T>>)_tree)[key];
            }

            set
            {
                ((IDictionary<string, List<T>>)_tree)[key] = value;
            }
        }

        public bool Contains(T item)
        {

            foreach (var list in _tree.Values)
            {
                if (list.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void Append(List<T> items, GhPath GhPath)
        {
            this.Append(items, GhPath.ToString());
        }

        public void Append(List<T> items, string GhPath)
        {

            if (!_tree.ContainsKey(GhPath))
            {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public void Append(T item, GhPath path)
        {
            this.Append(item, path.ToString());
        }

        public void Append(T item, string GhPath)
        {
            if (!_tree.ContainsKey(GhPath))
            {
                _tree.Add(GhPath, new List<T>());
            }
            _tree[GhPath].Add(item);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, List<T>>)_tree).ContainsKey(key);
        }

        public void Add(string key, List<T> value)
        {
            ((IDictionary<string, List<T>>)_tree).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, List<T>>)_tree).Remove(key);
        }

        public bool TryGetValue(string key, out List<T> value)
        {
            return ((IDictionary<string, List<T>>)_tree).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, List<T>> item)
        {
            ((IDictionary<string, List<T>>)_tree).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, List<T>>)_tree).Clear();
        }

        public bool Contains(KeyValuePair<string, List<T>> item)
        {
            return ((IDictionary<string, List<T>>)_tree).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, List<T>>[] array, int arrayIndex)
        {
            ((IDictionary<string, List<T>>)_tree).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, List<T>> item)
        {
            return ((IDictionary<string, List<T>>)_tree).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, List<T>>> GetEnumerator()
        {
            return ((IDictionary<string, List<T>>)_tree).GetEnumerator();
        }

        //IEnumerator IEnumerable.GetEnumerator() {
        //return ((IDictionary<string, List<T>>)_tree).GetEnumerator();
        //}
    }

}
