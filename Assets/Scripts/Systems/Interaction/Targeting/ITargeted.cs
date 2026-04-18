using System.Collections.Generic;
using Core.Commands;
using UnityEngine;

namespace Systems.Interaction.Targeting
{
	// 需要选目标格的道具/技能
	// 进入 ItemSelectionState 选中此类 Logic 后，跳转到 TargetingState 由玩家选目标格
	public interface ITargeted
	{
		// 返回所有合法的可选目标格
		IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx);

		// 额外校验（超出 GetValidCells 之外的约束）
		bool ValidateTarget(Vector2Int cell, InteractionContext ctx);

		IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell);

		ICommand CreateCommand(Vector2Int target, InteractionContext ctx);
	}
}
