using UnityEngine;

namespace Systems.Buff.Config
{
    public abstract class BuffEvent : ScriptableObject
    {
        public int priority = 0;
        public abstract void Trigger(BuffInfo buffInfo);
    }

    public abstract class UnitBuffEvent : BuffEvent 
    {
        protected Unit.Unit unit;

        public override void Trigger(BuffInfo buffInfo)
        {
            unit = (Unit.Unit)buffInfo.Target;
            Trigger(buffInfo, unit);
        }

        protected abstract void Trigger(BuffInfo buffInfo, Unit.Unit unit);

    }
}
