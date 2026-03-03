using Core.Events;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Reason why the turn order changed.
	/// </summary>
	public enum TurnOrderChangeReason
	{
		TurnReset,       // New turn started, queue rebuilt from scratch
		UnitAdvanced,    // Cursor advanced to next unit (NextUnit called)
		UnitAdded,       // Mid-turn addition (summon, reinforcement)
		UnitRemoved,     // Unit removed from queue (death, retreat)
		PriorityChanged, // Unit's action priority was modified
		SpeedChanged,    // Unit's speed changed externally, queue re-sorted
	}

	public readonly struct TurnOrderChangedEvent : IEvent
	{
		public TurnOrderChangeReason Reason { get; }

		/// <summary>
		/// relevant unit id for the change, if applicable (optional)
		/// </summary>
		public string AffectedUnitId { get; }

		public TurnOrderChangedEvent(TurnOrderChangeReason reason, string affectedUnitId = null)
		{
			Reason = reason;
			AffectedUnitId = affectedUnitId;
		}

		public override string ToString()
		{
			var unitInfo = string.IsNullOrEmpty(AffectedUnitId) ? "" : $" (Unit: {AffectedUnitId})";
			return $"[TurnOrderChanged] Reason: {Reason} {unitInfo}";
		}
	}
}
