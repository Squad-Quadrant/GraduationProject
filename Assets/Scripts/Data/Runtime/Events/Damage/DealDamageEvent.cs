using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Damage
{
    public struct DealDamageEvent : IEvent
    {
        public DamageTriggeringInfo Info; 
        public DealDamageEvent(DamageTriggeringInfo info)
        {
            this.Info = info;
        }
    }
}