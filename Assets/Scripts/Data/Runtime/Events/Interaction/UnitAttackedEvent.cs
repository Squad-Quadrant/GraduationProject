using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitAttackedEvent : IEvent
	{
        public Systems.Unit.Unit Attacker { get; }
        
        public UnitAttackedEvent(Systems.Unit.Unit attacker)
        {
            Attacker = attacker;
        }
	}
}
