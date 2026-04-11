using System;
using UnityEngine;

namespace Systems.Buff.Influence
{
    public abstract class BuffInfluence : ScriptableObject
    {
        public int priority = 0;
        public PropertyType propertyType;

        public abstract void Execute(BuffInfo buffInfo, BuffProperty property);
    }
    
    public abstract class BuffInfluence<T> : BuffInfluence where T : struct, IConvertible
    {
        public override void Execute(BuffInfo buffInfo, BuffProperty property)
        {
            Execute(buffInfo, (BuffProperty<T>)property);
        }

        protected abstract void Execute(BuffInfo buffInfo, BuffProperty<T> baseValue);
    }
    
    public abstract class UnitBuffInfluence<T> : BuffInfluence<T>  where T : struct, IConvertible
    {
        protected Unit.Unit unit;

        protected override void Execute(BuffInfo buffInfo, BuffProperty<T> property)
        {
            unit = (Unit.Unit)buffInfo.Target;
            Execute(buffInfo, property, unit);
        }

        protected abstract void Execute(BuffInfo buffInfo, BuffProperty<T> property, Unit.Unit unit);
    }
}