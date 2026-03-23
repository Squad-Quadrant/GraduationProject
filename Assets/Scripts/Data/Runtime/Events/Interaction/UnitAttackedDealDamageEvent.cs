using Core.Events;

namespace Data.Runtime.Events.Interaction
{
    public struct UnitAttackedDealDamageEvent : IEvent
    {
        public Systems.Unit.Unit Attacker { get; private set; }
        public Systems.Unit.Unit Target { get; private set; }
        public EActionType ActionType { get; private set; }
        
        public UnitAttackedDealDamageEvent(Systems.Unit.Unit attacker, Systems.Unit.Unit target, EActionType actionType)
        {
            Attacker = attacker;
            Target = target;
            ActionType = actionType;
        }
        
        // todo: 伤害计算，需要接入装备系统, 预计主要在UnitServer实现
    }
}