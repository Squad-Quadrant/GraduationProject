using Core.Events;

namespace Data.Runtime.Events.Map
{
    public struct UpdateGunLineEvent : IEvent
    {
        public Systems.Unit.Unit attacker;
        public Systems.Unit.Unit target;

        public UpdateGunLineEvent(Systems.Unit.Unit attacker, Systems.Unit.Unit target)
        {
            this.attacker = attacker;
            this.target = target;
        }
    }
}