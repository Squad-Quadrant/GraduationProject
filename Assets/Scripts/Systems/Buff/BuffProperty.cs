using System;

namespace Systems.Buff
{
    public enum PropertyType
    {
        None = 0,
        Attack,
        Defense,
        MaxHp,
        Speed,
        MaxAmmo,
        Count
    }

    public abstract class BuffProperty
    {
        protected PropertyType type;
        protected IBuffAble owner;
        
        public PropertyType  Type => type;
        public IBuffAble Owner => owner;
    }

    public class BuffProperty<T> : BuffProperty where T : struct, IConvertible
    {
        private T _baseValue;
        
        public T BaseValue => _baseValue;
        
        public BuffProperty(PropertyType propertyType, T baseValue, IBuffAble owner)
        {
            type = propertyType;
            _baseValue = baseValue;
            this.owner = owner;
        }

        public T buffValue; // BuffInfluence中对该值处理

        public T Value
        {
            get
            {
                buffValue = _baseValue;
                owner.BuffProxy.ExecutePropertyInfluence(this);
                return buffValue;
            }
        }

        public static implicit operator T(BuffProperty<T> property)
        {
            if (property == null) return default;
            return property.Value;
        }
    }
}