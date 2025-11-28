using Core.Events;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - All units have completed their actions for the current turn.
	///  - Call EndTurn() manually.
	/// </summary>
	public readonly struct TurnEndedEvent : IEvent
	{
		public int TurnNumber { get; }

		public TurnEndedEvent(int turnNumber) => TurnNumber = turnNumber;

		public override string ToString() => $"[TurnEnded] Turn {TurnNumber}";
	}
}
