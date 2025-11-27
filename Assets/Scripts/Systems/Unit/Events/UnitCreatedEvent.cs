using Core.Events;

namespace Systems.Unit.Events
{
	/// <summary>
	/// Triggered when a unit is created in the game.
	/// </summary>
	public readonly struct UnitCreatedEvent : IEvent
	{
		public Unit Unit { get; }

		public UnitCreatedEvent(Unit unit) => Unit = unit;

		public override string ToString() => $"[UnitCreated] {Unit.Name}({Unit.Id}) at {Unit.Position}";
	}
}
