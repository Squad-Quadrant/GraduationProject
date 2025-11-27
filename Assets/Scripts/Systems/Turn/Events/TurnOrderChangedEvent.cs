using Core.Events;

namespace Systems.Turn.Events
{
	/// <summary>
	/// Reason why the turn order changed.
	/// </summary>
	public enum TurnOrderChangeReason
	{
		SpeedModified,	// e.g., buff/debuff affecting unit speed
		UnitInserted,	// e.g., skill "act immediately"
		UnitAdded,		// e.g., summon or new unit joins battle
		UnitRemoved,	// e.g., unit defeated or leaves battle
		TurnReset		// e.g., end of round resetting turn order
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
