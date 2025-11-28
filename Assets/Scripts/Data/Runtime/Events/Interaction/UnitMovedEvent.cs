using System.Collections.Generic;
using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	/// <summary>
	/// The logical position is already updated when this fires. This event is for triggering visual/audio feedback.
	/// </summary>
	public readonly struct UnitMovedEvent : IEvent
	{
		public string UnitId { get; }

		public Vector2Int FromPosition { get; }

		public Vector2Int ToPosition { get; }

		public IReadOnlyList<Vector2Int> Path { get; }

		public UnitMovedEvent(
			string unitId,
			Vector2Int fromPosition,
			Vector2Int toPosition,
			IReadOnlyList<Vector2Int> path = null)
		{
			UnitId = unitId;
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Path = path;
		}

		public override string ToString() => $"[UnitMoved] {UnitId}: {FromPosition} -> {ToPosition}";
	}
}
