using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Damage
{
    public struct DamageAppliedEvent : IEvent
    {
        public DamageExecutingContext Context { get;}
        
        public DamageAppliedEvent(DamageExecutingContext context)
        {
            Context = context;
        }
    }
}