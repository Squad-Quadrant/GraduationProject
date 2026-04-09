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

    public class BuffProperty<T> where T : struct, IConvertible
    {
        private PropertyType _type;
        private T _baseValue;
        private IBuffAble _owner;
        
        public PropertyType  Type => _type;
        public T BaseValue => _baseValue;
        public IBuffAble Owner => _owner;
        
        public BuffProperty(PropertyType propertyType, T baseValue, IBuffAble owner)
        {
            _type = propertyType;
            _baseValue = baseValue;
            _owner = owner;
        }

        public T Value 
        {
            get => _owner.BuffProxy.Property(_type, _baseValue);

            set => _baseValue = value;
        }

        public static implicit operator T(BuffProperty<T> property)
        {
            if (property == null) return default;
            return property.Value;
        }
    }
}