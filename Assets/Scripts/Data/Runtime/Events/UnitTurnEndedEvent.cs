using Core.Events;

namespace Data.Runtime.Events
{
	/// <summary>
	/// Triggered when:
	///  - A unit's turn ends.
	///  - Call EndUnitTurn() manually.
	/// </summary>
	public readonly struct UnitTurnEndedEvent : IEvent
	{
		public string UnitId { get; }

		public int TurnNumber { get; }

		public UnitTurnEndedEvent(string unitId, int turnNumber)
		{
			UnitId = unitId;
			TurnNumber = turnNumber;
		}

		public override string ToString() => $"[UnitTurnEnded] Unit '{UnitId}' finished on Turn {TurnNumber}";
	}
}
