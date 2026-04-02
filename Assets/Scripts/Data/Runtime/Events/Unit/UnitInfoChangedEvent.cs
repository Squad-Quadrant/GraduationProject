using Core.Events;

namespace Data.Runtime.Events.Unit
{
	public readonly struct UnitInfoChangedEvent : IEvent
	{
		public Systems.Unit.Unit Unit { get; }

		public UnitInfoChangedEvent(Systems.Unit.Unit unit) => Unit = unit;

		public override string ToString() => $"[UnitInfoChanged] {Unit.id}";
	}
}
