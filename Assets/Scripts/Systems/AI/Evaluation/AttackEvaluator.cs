using System.Collections.Generic;
using Core.Log;
using Data.Runtime;
using Systems.Equipment;
using UnityEngine;

namespace Systems.AI.Evaluation
{
	public class AttackEvaluator : IActionEvaluator
	{
		private const float DefaultBaseScore = 0.7f;
        private const float Fear = 0.5f;
        private const float KillAwareness = 0.5f;
        private const float Bluntness = 0.01f;
        private const float Tactics = 0.1f;

		public List<AIActionOption> Evaluate(AIContext context)
		{
			var results = new List<AIActionOption>();
			var unit = context.Self;
			var brain = context.Brain;
            var currentEquipment = unit.CurrentEquipment;
            
            if (currentEquipment.IsNullOrEmpty()) return results;
			if (context.Enemies.Count == 0) return results;

			float baseScore = brain ? brain.attackBase : DefaultBaseScore;
			float fear = brain ? brain.fear : Fear;
            float killAwareness = brain ? brain.killAwareness : KillAwareness;
            float bluntness = brain ? brain.bluntness : Bluntness;
            float tactics = brain ? brain.tacticsAttack : Tactics;
            
            Unit.Unit bestTarget = null;
			float bestScore = float.MinValue;
            List<Unit.Unit> attackableUnits = context.Enemies.FindAll(u => currentEquipment.Logic.CheckAttackable(u));
			foreach (var target in attackableUnits)
            {
                float fearScore = fear * (1f - (float)unit.CurrentHp / unit.maxHp);
                float killScore = killAwareness * (1f - (float)target.CurrentHp / target.maxHp);
                float bluntnessScore = bluntness * ManhattanDistance(unit.position, target.position);
                float tacticsScore = tactics * unit.CurrentAp;
                float score = baseScore - fearScore + killScore - bluntnessScore + tacticsScore;
                
                if (!(score > bestScore)) continue;
                bestScore = score;
                bestTarget = target;
            }

			if (!(bestScore > float.MinValue)) return results;
			this.Log($"Best attack target: {bestTarget.name} with score {bestScore}");
            results.Add(new AIActionOption(EAIActionType.Attack, bestScore)
            {
	            TargetUnitId = bestTarget.id,
                EquipmentAction = EActionType.Attack
            });

            return results;
		}

		private static int ManhattanDistance(Vector2Int a, Vector2Int b)
			=> Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}
}
