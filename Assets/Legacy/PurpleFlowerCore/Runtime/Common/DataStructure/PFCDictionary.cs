using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurpleFlowerCore
{
    [Serializable]
    public class PFCDictionary<TK, TV> : IDictionary<TK, TV> where TK : IEquatable<TK>
    {
        //[SerializeField] private HashSet<KeyValuePair<TK, TV>> data = new();
        [SerializeField] private List<KeyValuePair<TK, TV>> data = new();
        
        public int Count => data.Count;
        public bool IsReadOnly { get; }
        public ICollection<TK> Keys => GetKeys();
        public ICollection<TV> Values => GetValues();

        public void Add(TK key, TV value)
        {
            Add(new KeyValuePair<TK, TV>(key, value));
        }

        public void Add(KeyValuePair<TK, TV> kv)
        {
            if(ContainsKey(kv.Key))
                SetValue(kv.Key, kv.Value);
            else
                data.Add(kv);
        }
        
        public void Add(System.Collections.Generic.KeyValuePair<TK, TV> item)
        {
            Add(item);
        }

        public bool Remove(TK key)
        {
            return data.Remove(GetKV(key));
        }

        public bool TryGetValue(TK key, out TV value)
        {
            if (ContainsKey(key))
            {
                value = GetValue(key);
                return true;
            }
            value = default;
            return false;
        }

        public TV this[TK key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public IEnumerator<System.Collections.Generic.KeyValuePair<TK, TV>> GetEnumerator()
        {
            throw new Exception();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(System.Collections.Generic.KeyValuePair<TK, TV> item)
        {
            return data.Any(d => d.Key.Equals(item.Key) && d.Value.Equals(item.Value));
        }
        
        public bool ContainsKey(TK key)
        {
            return data.Any(d => d.Key.Equals(key));
        }

        public void CopyTo(System.Collections.Generic.KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(System.Collections.Generic.KeyValuePair<TK, TV> item)
        {
            foreach (var kv in data.Where(kv => kv.Key.Equals(item.Key) && kv.Value.Equals(item.Value)))
            {
                data.Remove(kv);
                return true;
            }

            return false;
        }

        public List<TK> GetKeys()
        {
            List<TK> res = new(Count);
            for (int i = 0; i < Count; i++)
            {
                res[i] = data[i].Key;
            }
            return res;
        }

        public List<TV> GetValues()
        {
            List<TV> res = new(Count);
            for (int i = 0; i < Count; i++)
            {
                res[i] = data[i].Value;
            }
            return res;
        }
        
        private TV GetValue(TK key)
        {
            return GetKV(key).Value;
        }
        
        private TK GetKey(TV v)
        {
            return GetKV(v).Key;
        }

        private KeyValuePair<TK, TV> GetKV(TK key)
        {
            foreach (var kv in data.Where(kv => kv.Key.Equals(key)))
            {
                return kv;
            }
            throw new Exception();
        }
        
        private KeyValuePair<TK, TV> GetKV(TV value)
        {
            foreach (var kv in data.Where(kv => kv.Value.Equals(value)))
            {
                return kv;
            }
            throw new Exception();
        }
        
        private void SetValue(TK key, TV value)
        {
            var kv = GetKV(key);
            kv.Value = value;
        }
    }

    // class PFCEnumerator : IEnumerator
    // {
    //     
    //     public bool MoveNext()
    //     {
    //         
    //     }
    //
    //     public void Reset()
    //     {
    //         
    //     }
    //
    //     public object Current { get; }
    // }

    // public TV this[TK index]
    // {
    //     get => GetValue(index);
    //
    //     set
    //     {
    //
    //     }
    // }

    // public ICollection<TK> Keys { get; }
    // public ICollection<TV> Values { get; }

    // public void Contains(TK k)
    // {
    //     foreach (var kv in data)
    //     {
    //         if (kv.k == k)
    //             return
    //     }
    // }

    // private void Check()
    // {
    //     if (keys.Count != values.Count)
    //         throw new Exception("There are some errors in PFCDictionary");
    // }
    //
    // private void Check(TK key)
    // {
    //     Check();
    //     if (key == null) return;
    //     if (!keys.Contains(key))
    //         throw new Exception($"PFCDictionary keys are not contain {key}");
    // }

    // private void Check(TV value)
    // {
    //     Check();
    //     if (value == null) return;
    //     if (!values.Contains(value))
    //         throw new Exception($"PFCDictionary values are not contain {value}");
    // }

    // private void Check(TK k, TV v)
    // {
    //     Check();
    //     Check(k);
    //     Check(v);
    // }


    //
    // private void AddKV(TK k, TV v)
    // {
    //     Check();
    //     if (keys.Contains(k))
    //     {
    //         values.RemoveAt(keys.IndexOf(k));
    //         keys.Remove(k);
    //     }
    //
    //     keys.Add(k);
    //     values.Add(v);
    // }


}