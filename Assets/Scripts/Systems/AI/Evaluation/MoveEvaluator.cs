using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Systems.Unit.Equipment;
using UnityEngine;

namespace Systems.AI.Evaluation
{
	public class MoveEvaluator : IActionEvaluator
	{
		private const float DefaultBaseScore = 0.3f;
		private const float DefaultClosenessWeight = 0.2f;
		private const float DefaultInRangeBonus = 0.2f;
		private const float DefaultApConservationBonus = 0.1f;


		public List<AIActionOption> Evaluate(AIContext context)
		{
			var results = new List<AIActionOption>();
			var unit = context.Self;
			var brain = context.Brain;

			if (context.Enemies.Count == 0) return results; // todo: 目前设定为看不见玩家就不动

			float baseScore = brain ? brain.moveBase : DefaultBaseScore;
			float closenessW = brain ? brain.closenessWeight : DefaultClosenessWeight;
			float inRangeB = brain ? brain.inRangeBonus : DefaultInRangeBonus;
			float apBonus = brain ? brain.apConservationBonus : DefaultApConservationBonus;

			var reachable = context.ReachableArea;
			int effectiveAttackRange = GetEffectiveAttackRange(unit);
			float maxRelevantDistance = unit.moveRange * unit.CurrentAp + 10f;

			Vector2Int bestCell = unit.position;
			float bestScore = float.MinValue;

			foreach (var cell in reachable.StoppableCells)
            {
                if (cell == unit.position) continue;

                int pathCost = reachable.GetCostTo(cell);
                int apCost = unit.CalculateMovementApCost(pathCost);
                int apRemaining = unit.CurrentAp - apCost;

                int nearestDist = int.MaxValue;
                bool anyInRange = false; // 对当前格子来说，有没有在攻击距离内的
                foreach (var enemy in context.Enemies)
                {
	                int dist = ManhattanDistance(cell, enemy.position);
	                if (dist < nearestDist) nearestDist = dist;
	                if (dist <= effectiveAttackRange) anyInRange = true;
                }

                float closeness = 1f - Mathf.Clamp01(nearestDist / maxRelevantDistance);

                float score = baseScore
                            + closenessW * closeness
                            + (anyInRange ? inRangeB : 0f)
                            + (apRemaining > 0 ? apBonus : 0f);

                if (!(score > bestScore)) continue;
                bestScore = score;
                bestCell = cell;
            }

			if (!(bestScore > float.MinValue)) return results;
			this.Log($"Best move: {bestCell}, score: {bestScore:F2}");
            results.Add(new AIActionOption(EAIActionType.Move, bestScore)
            {
	            MoveTarget = bestCell
            });

            return results;
		}

		private static int GetEffectiveAttackRange(Unit.Unit unit) // 计算有效攻击距离
		{
			var weapon = unit.CurrentEquipment;
			return weapon.IsNullOrEmpty() ? 0 : Mathf.Min(weapon.Logic.Range(), unit.visionRange);
		}

		private static int ManhattanDistance(Vector2Int a, Vector2Int b)
			=> Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}
}
