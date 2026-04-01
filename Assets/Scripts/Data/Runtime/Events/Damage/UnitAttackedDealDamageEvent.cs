using Core.Events;

namespace Data.Runtime.Events.Damage
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
    }
}