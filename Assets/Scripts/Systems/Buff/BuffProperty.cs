using System;

namespace Systems.Buff
{
    public enum PropertyType
    {
        None = 0,
        // ---------------------------------
        Attack,
        Defense,
        MaxHp,
        Speed,
        MoveRange,
        MaxAmmo,
        CanUseMainWeapon,
        CanAttack,
        VisionRange,
        CanAIUseEye,
        // ---------------------------------
        Count,
    }

    public abstract class BuffProperty
    {
        protected PropertyType type;
        protected IBuffAble owner;
        
        public PropertyType  Type => type;
        public IBuffAble Owner => owner;

        public void SetOwner(IBuffAble newOwner)
        {
            owner = newOwner;
        }
    }

    public class BuffProperty<T> : BuffProperty where T : struct, IConvertible
    {
        private T _baseValue;
        
        public T BaseValue => _baseValue;
        
        public BuffProperty(PropertyType propertyType, T baseValue)
        {
            type = propertyType;
            _baseValue = baseValue;
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
            
            set => _baseValue = value;
        }

        public static implicit operator T(BuffProperty<T> property)
        {
            if (property == null) return default;
            return property.Value;
        }
    }

    public static class BuffPropertyInjector
    {
        public static void Inject(IBuffAble owner)
        {
            if (owner == null) return;
            var type = owner.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            
            foreach (var field in type.GetFields(flags))
            {
                if (typeof(BuffProperty).IsAssignableFrom(field.FieldType))
                {
                    (field.GetValue(owner) as BuffProperty)?.SetOwner(owner);
                }
            }
            
            foreach (var prop in type.GetProperties(flags))
            {
                if (typeof(BuffProperty).IsAssignableFrom(prop.PropertyType) && prop.CanRead)
                {
                    (prop.GetValue(owner) as BuffProperty)?.SetOwner(owner);
                }
            }
        }
    }
}