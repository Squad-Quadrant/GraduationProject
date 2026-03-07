using Core.Events;

namespace Data.Runtime.Events.Interaction
{
    public struct UnitDealDamageEvent : IEvent
    {
        public Systems.Unit.Unit Attacker { get; private set; }
        public Systems.Unit.Unit Target { get; private set; }
        
        public UnitDealDamageEvent(Systems.Unit.Unit attacker, Systems.Unit.Unit target)
        {
            Attacker = attacker;
            Target = target;
        }
        
        // todo: 伤害计算，需要接入装备系统, 预计主要在UnitServer实现
    }
}