using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Input
{
	/// <summary>
	/// This event is only published when the hovered cell changes, not every frame.
	/// </summary>
	public readonly struct PointerHoverEvent : IEvent
	{
		/// <summary>
		/// Null if pointer is outside the map bounds.
		/// </summary>
		public Vector2Int? CellPosition { get; }

		public Vector3 WorldPosition { get; }

		public string HoveredUnitId { get; }

		public bool IsOverMap => CellPosition.HasValue;

		public PointerHoverEvent(Vector2Int? cellPosition, Vector3 worldPosition, string hoveredUnitId = null)
		{
			CellPosition = cellPosition;
			WorldPosition = worldPosition;
			HoveredUnitId = hoveredUnitId;
		}

		public override string ToString()
		{
			var cellInfo = CellPosition.HasValue ? CellPosition.Value.ToString() : "Outside";
			var unitInfo = HoveredUnitId != null ? $", Unit:{HoveredUnitId}" : "";
			return $"[PointerHover] Cell:{cellInfo}{unitInfo}";
		}
	}
}
