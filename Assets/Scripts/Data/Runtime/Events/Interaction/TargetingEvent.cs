using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct TargetingEvent : IEvent
	{
		public Vector2Int? TargetCell { get; }

		public TargetingEvent(Vector2Int? targetCell) => TargetCell = targetCell;

		public static TargetingEvent Clear() => new(null);
	}
}
