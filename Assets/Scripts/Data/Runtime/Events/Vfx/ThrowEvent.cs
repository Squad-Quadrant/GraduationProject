using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Vfx
{
	public readonly struct ThrowEvent : IEvent
	{
		public string OwnerUnitId { get; }
		public Vector2Int TargetCell { get; }
		public GameObject ProjectilePrefab { get; }

		public ThrowEvent(string ownerUnitId, Vector2Int targetCell, GameObject projectilePrefab)
		{
			OwnerUnitId = ownerUnitId;
			TargetCell = targetCell;
			ProjectilePrefab = projectilePrefab;
		}

		public override string ToString() =>
			$"[Throw] {OwnerUnitId} → {TargetCell} (prefab={ProjectilePrefab?.name ?? "null"})";
	}
}
