using Core.Events;
using Systems.Unit;

namespace Data.Runtime.Events
{
	public readonly struct UnitCreatedEvent : IEvent
	{
		public Unit Unit { get; }

		public UnitCreatedEvent(Unit unit) => Unit = unit;

		public override string ToString() => $"[UnitCreated] {Unit.name}({Unit.Id}) at {Unit.position}";
	}
}
