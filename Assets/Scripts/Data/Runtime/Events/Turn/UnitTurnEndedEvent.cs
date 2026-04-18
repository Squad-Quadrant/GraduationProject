using Core.Events;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - A unit's turn ends.
	///  - Call EndUnitTurn() manually.
	/// </summary>
	public readonly struct UnitTurnEndedEvent : IEvent
	{
		public string TurnUnitId { get; }

		public int TurnNumber { get; }

		public UnitTurnEndedEvent(string turnUnitId, int turnNumber)
		{
			TurnUnitId = turnUnitId;
			TurnNumber = turnNumber;
		}

		public override string ToString() => $"[UnitTurnEnded] Unit '{TurnUnitId}' finished on Turn {TurnNumber}";
	}
}
