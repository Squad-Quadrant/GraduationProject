using Core.Events;

namespace Data.Runtime.Events.Interaction
{
    public readonly struct UnitReloadedEvent : IEvent
    {
        public Systems.Unit.Unit Unit { get; }

        public UnitReloadedEvent(Systems.Unit.Unit unit)
        {
            Unit = unit;
        }

        public override string ToString() => $"[UnitReloadedEvent] Unit: {Unit.id}";
    }
}