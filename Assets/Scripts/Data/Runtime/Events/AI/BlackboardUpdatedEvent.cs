using Core.Events;
using Systems.Unit;

namespace Data.Runtime.Events.AI
{
	public readonly struct BlackboardUpdatedEvent : IEvent
	{
		public EUnitFaction Faction { get; }

		public BlackboardUpdatedEvent(EUnitFaction faction)
		{
			Faction = faction;
		}

		public override string ToString() => $"[BlackboardUpdated] {Faction}";
	}
}
