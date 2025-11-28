using Core.Events;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - The first turn of the game starts.
	///  - All units have completed their actions for the current turn, and a new turn is beginning.
	/// </summary>
	public readonly struct TurnStartedEvent : IEvent
	{
		public int TurnNumber { get; }

		public TurnStartedEvent(int turnNumber) => TurnNumber = turnNumber;

		public override string ToString() => $"[TurnStarted] Turn {TurnNumber}.";
	}
}
