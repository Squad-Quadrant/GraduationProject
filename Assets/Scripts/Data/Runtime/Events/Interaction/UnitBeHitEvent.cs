using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitBeHitEvent : IEvent
	{
        public Systems.Unit.Unit Unit { get; }
        public UnitBeHitEvent(Systems.Unit.Unit unit)
        {
            Unit = unit;
        }
	}
}
