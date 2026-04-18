using Core.Events;

namespace Data.Runtime.Events.UI
{
	public readonly struct TacticalItemSelectedEvent : IEvent
	{
		public int SlotIndex { get; }

		public TacticalItemSelectedEvent(int slotIndex) => SlotIndex = slotIndex;

		public override string ToString() => $"[TacticalItemSelected] Slot:{SlotIndex}";
	}
}
