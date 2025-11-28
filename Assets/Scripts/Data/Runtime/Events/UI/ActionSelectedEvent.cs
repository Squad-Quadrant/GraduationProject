using Core.Events;

namespace Data.Runtime.Events.UI
{
	/// <summary>
	/// Published by UI when player selects an action from the action menu.
	///
	/// Example flow:
	/// 1. Player clicks on their unit → UnitSelectedState shows action menu
	/// 2. Player clicks "Move" button → ActionSelectedEvent(Move) published
	/// 3. State machine transitions to MovementPreviewState
	/// </summary>
	public readonly struct ActionSelectedEvent : IEvent
	{
		public EActionType ActionType { get; }

		/// <summary>
		/// Optional: ID of a specific skill or item selected.
		/// </summary>
		public string ActionId { get; }

		public ActionSelectedEvent(EActionType actionType, string actionId = null)
		{
			ActionType = actionType;
			ActionId = actionId;
		}

		public override string ToString()
		{
			var idInfo = ActionId != null ? $", Id:{ActionId}" : "";
			return $"[ActionSelected] {ActionType}{idInfo}";
		}
	}
}
