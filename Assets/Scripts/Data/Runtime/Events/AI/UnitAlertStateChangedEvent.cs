using Core.Events;
using Systems.AI.Alert;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Events.AI
{
	public readonly struct UnitAlertStateChangedEvent : IEvent
	{
		public string UnitId { get; }
		public EUnitFaction Faction { get; }
		public EAlertLevel From { get; }
		public EAlertLevel To { get; }
		public Vector2Int Position { get; }

		public UnitAlertStateChangedEvent(
			string unitId,
			EUnitFaction faction,
			EAlertLevel from,
			EAlertLevel to,
			Vector2Int position)
		{
			UnitId = unitId;
			Faction = faction;
			From = from;
			To = to;
			Position = position;
		}

		public override string ToString() =>
			$"[AlertChanged] {UnitId} ({Faction}): {From} → {To} @ {Position}";
	}
}
