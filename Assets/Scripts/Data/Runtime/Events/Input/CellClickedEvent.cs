using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Input
{
	public readonly struct CellClickedEvent : IEvent
	{
		public Vector2Int CellPosition { get; }

		public Vector3 WorldPosition { get; }

		public CellClickedEvent(Vector2Int cellPosition, Vector3 worldPosition)
		{
			CellPosition = cellPosition;
			WorldPosition = worldPosition;
		}

		public override string ToString() => $"[CellClicked] Cell: {CellPosition}";
	}
}
