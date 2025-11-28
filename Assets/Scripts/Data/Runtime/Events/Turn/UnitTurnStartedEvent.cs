using Core.Events;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - A unit's turn starts.
	///  - Call NextUnit() manually.
	/// </summary>
	public readonly struct UnitTurnStartedEvent : IEvent
	{
		public string UnitId { get; }

		public int TurnNumber { get; }

		public UnitTurnStartedEvent(string unitId, int turnNumber)
		{
			UnitId = unitId;
			TurnNumber = turnNumber;
		}

		public override string ToString() => $"[UnitTurnStarted] Unit '{UnitId}' on Turn {TurnNumber}";
	}
}
