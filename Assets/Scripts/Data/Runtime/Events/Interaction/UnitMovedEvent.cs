using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitMovedEvent : IEvent
	{
		public Systems.Unit.Unit Unit { get; }

		public Vector2Int FromPosition { get; }

		public Vector2Int ToPosition { get; }

		public IReadOnlyList<Vector2Int> Path { get; }

		public readonly float MovementSpeedMultiplier;

		public UnitMovedEvent(
			Systems.Unit.Unit unit,
			Vector2Int fromPosition,
			Vector2Int toPosition,
			IReadOnlyList<Vector2Int> path = null,
			float movementSpeedMultiplier = 1f)
		{
			Unit = unit;
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Path = path;
			MovementSpeedMultiplier = Mathf.Max(0.01f, movementSpeedMultiplier);
		}

		public override string ToString() => $"[UnitMoved] {Unit.id}: {FromPosition} -> {ToPosition}";
	}
}
