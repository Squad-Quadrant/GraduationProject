using Core.Events;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Events.Turn
{
	/// <summary>
	/// Triggered when:
	///  - A unit's turn starts.
	///  - Call NextUnit() manually.
	/// </summary>
	public readonly struct UnitTurnStartedEvent : IEvent
	{
		public string TurnUnitId { get; }

		public string DisplayName { get; }

		public int TurnNumber { get; }

		public bool IsVisibleToPlayer { get; } // 用于触发聚焦

		public Vector2Int CellPosition { get; }

		public UnitTurnStartedEvent(string turnUnitId, string displayName, int turnNumber, bool isVisibleToPlayer, Vector2Int cellPosition)
		{
			TurnUnitId = turnUnitId;
			DisplayName = displayName;
			TurnNumber = turnNumber;
			IsVisibleToPlayer = isVisibleToPlayer;
			CellPosition = cellPosition;
		}

		public override string ToString() => $"[UnitTurnStarted] Unit '{TurnUnitId}' on Turn {TurnNumber}";
	}
}
