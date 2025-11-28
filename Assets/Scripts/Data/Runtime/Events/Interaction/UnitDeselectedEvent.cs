using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitDeselectedEvent : IEvent
	{
		public string UnitId { get; }

		public UnitDeselectedEvent(string unitId)
		{
			UnitId = unitId;
		}

		public override string ToString() => $"[UnitDeselected] {UnitId ?? "None"}";
	}
}
