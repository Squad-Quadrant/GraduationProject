using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.UI
{
	/// <summary>
	/// Published when player confirms a target for the current action.
	///
	/// Example flow:
	/// 1. MovementPreviewState: Player sees highlighted movable cells
	/// 2. Player clicks on a valid cell → CellClickedEvent
	/// 3. State validates the target
	/// 4. If valid, state may show a confirmation UI
	/// 5. Player confirms → TargetConfirmedEvent
	/// 6. State creates and executes the command
	/// </summary>
	public readonly struct TargetConfirmedEvent : IEvent
	{
		public EActionType ActionType { get; }

		public Vector2Int? TargetCell { get; }

		public string TargetUnitId { get; }

		public TargetConfirmedEvent(EActionType actionType, Vector2Int? targetCell = null, string targetUnitId = null)
		{
			ActionType = actionType;
			TargetCell = targetCell;
			TargetUnitId = targetUnitId;
		}

		public override string ToString()
		{
			var cellInfo = TargetCell.HasValue ? $", Cell:{TargetCell}" : "";
			var unitInfo = TargetUnitId != null ? $", Target:{TargetUnitId}" : "";
			return $"[TargetConfirmed] {ActionType}{cellInfo}{unitInfo}";
		}
	}
}
