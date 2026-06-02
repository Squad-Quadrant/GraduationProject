using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitInspectedEvent : IEvent
	{
		public string UnitId { get; }

		public UnitInspectedEvent(string unitId) => UnitId = unitId;

		public override string ToString() => $"[UnitInspected] {UnitId}";
	}
}
