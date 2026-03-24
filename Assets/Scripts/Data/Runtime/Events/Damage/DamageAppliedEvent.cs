using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Damage
{
    public class DamageAppliedEvent : IEvent
    {
        public DamageExecutingContext Context { get;}
        
        public DamageAppliedEvent(DamageExecutingContext context)
        {
            Context = context;
        }
    }
}