using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Input
{
	public readonly struct CellClickedEvent : IEvent
	{
		public Vector2Int CellPosition { get; }

		public Vector3 WorldPosition { get; }

		/// <summary>
		/// 0 = Left, 1 = Right, 2 = Middle
		/// </summary>
		public int MouseButton { get; }

		public CellClickedEvent(Vector2Int cellPosition, Vector3 worldPosition, int mouseButton = 0)
		{
			CellPosition = cellPosition;
			WorldPosition = worldPosition;
			MouseButton = mouseButton;
		}

		public override string ToString() => $"[CellClicked] Cell: {CellPosition}, Button: {MouseButton}";
	}
}
