using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Damage
{
    public struct RecoverAppliedEvent : IEvent
    {
        public DamageExecutingContext Context { get;}
        
        public RecoverAppliedEvent(DamageExecutingContext context)
        {
            Context = context;
        }
    }
}