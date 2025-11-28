using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Input
{
	/// <summary>
	/// This event is published instead of CellClickedEvent when a unit is clicked.
	/// </summary>
	public readonly struct UnitClickedEvent : IEvent
	{
		public string UnitId { get; }

		public Vector2Int CellPosition { get; }

		public Vector3 WorldPosition { get; }

		/// <summary>
		/// 0 = Left, 1 = Right, 2 = Middle
		/// </summary>
		public int MouseButton { get; }

		public UnitClickedEvent(string unitId, Vector2Int cellPosition, Vector3 worldPosition, int mouseButton = 0)
		{
			UnitId = unitId;
			CellPosition = cellPosition;
			WorldPosition = worldPosition;
			MouseButton = mouseButton;
		}

		public override string ToString() => $"[UnitClicked] Unit:{UnitId}, Cell:{CellPosition}, Button:{MouseButton}";
	}
}
