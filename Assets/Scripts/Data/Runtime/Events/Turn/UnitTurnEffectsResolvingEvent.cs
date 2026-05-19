using Core.Events;

namespace Data.Runtime.Events.Turn
{
	public readonly struct UnitTurnEffectsResolvingEvent : IEvent
	{
		public string TurnUnitId { get; }

		public int TurnNumber { get; }

		public UnitTurnEffectsResolvingEvent(string turnUnitId, int turnNumber)
		{
			TurnUnitId = turnUnitId;
			TurnNumber = turnNumber;
		}

		public override string ToString() => $"[UnitTurnEffectsResolving] Unit '{TurnUnitId}' on Turn {TurnNumber}";
	}
}
