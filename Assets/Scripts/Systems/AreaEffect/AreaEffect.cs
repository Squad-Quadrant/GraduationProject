using System.Collections.Generic;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Systems.AreaEffect
{
	// 地图上的持续效果实例，作为 ITurnUnit 插入 TurnQueue 与 Unit 共用回合推进
	public class AreaEffect
	{
		public string Id { get; }
		public string OwnerId { get; }
		public Vector2Int TargetCell { get; }
		public IReadOnlyList<Vector2Int> Cells { get; }
		public AreaEffectBehavior Behavior { get; }
		public int RemainingTurns { get; internal set; } // >= 0 时 OnTurnStart；< 0 时 OnExpired + Unregister

		public AreaEffect(
			string id,
			string ownerId,
			Vector2Int targetCell,
			IReadOnlyList<Vector2Int> cells,
			int initialRemainingTurns,
			AreaEffectBehavior behavior)
		{
			Id = id;
			OwnerId = ownerId;
			TargetCell = targetCell;
			Cells = cells;
			RemainingTurns = initialRemainingTurns;
			Behavior = behavior;
		}

		public override string ToString() =>
			$"[AreaEffect] {Behavior.DisplayName}({Id}) @{TargetCell} cells:{Cells.Count} remaining:{RemainingTurns}";
	}
}
