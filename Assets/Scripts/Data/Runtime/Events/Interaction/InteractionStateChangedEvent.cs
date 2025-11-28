using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	/// <summary>
	/// high-level notification for UI and other systems that need to react to player interaction flow changes.
	/// </summary>
	public readonly struct InteractionStateChangedEvent : IEvent
	{
		public string PreviousState { get; }

		public string CurrentState { get; }

		public string SelectedUnitId { get; }

		public InteractionStateChangedEvent(
			string previousState,
			string currentState,
			string selectedUnitId = null)
		{
			PreviousState = previousState;
			CurrentState = currentState;
			SelectedUnitId = selectedUnitId;
		}

		public override string ToString() =>
			$"[InteractionState] {PreviousState} -> {CurrentState}";
	}
}
