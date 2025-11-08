using System;
using UnityEngine.Serialization;

namespace PurpleFlowerCore
{
    [Serializable]
    public record KeyValuePair<TK, TV> where TK : IEquatable<TK>
    {
        public TK Key;
        public TV Value;
        
        public KeyValuePair(TK key, TV value)
        {
            Key = key;
            Value = value;
        }
        
        public KeyValuePair()
        {
            Key = default;
            Value = default;
        }
        
        public static implicit operator KeyValuePair<TK, TV>((TK, TV) kvt)
        {
            return new KeyValuePair<TK, TV>
            {
                Key = kvt.Item1,
                Value = kvt.Item2
            };
        }
        
        public static implicit operator KeyValuePair<TK, TV>(System.Collections.Generic.KeyValuePair<TK, TV> kvp)
        {
            return new KeyValuePair<TK, TV>
            {
                Key = kvp.Key,
                Value = kvp.Value
            };
        }
    }
}