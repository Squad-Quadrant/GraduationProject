using Core.Events;

namespace Data.Runtime.Events.Unit
{
	/// <summary>
	/// Triggered when a unit is created in the game.
	/// </summary>
	public readonly struct UnitCreatedEvent : IEvent
	{
		public Systems.Unit.Unit Unit { get; }

		public UnitCreatedEvent(Systems.Unit.Unit unit) => Unit = unit;

		public override string ToString() => $"[UnitCreated] {Unit.Name}({Unit.Id}) at {Unit.Position}";
	}
}
