using System.Collections.Generic;
using Core.Log;
using Data.Runtime;
using Systems.AI.Actions;
using Systems.Unit.Equipment;
using UnityEngine;

namespace Systems.AI.Plans
{
	public class EngagePlan : ITurnPlan
	{
		private readonly Unit.Unit _target;

		public string Name => $"Engage({_target.id})";

		public EngagePlan(Unit.Unit target) => _target = target;

		public bool IsViable(AIContext context)
		{
			if (_target is not { IsAlive: true }) return false;

			var weapon = context.Self.CurrentWeaponContainer;
			if (weapon.IsNullOrEmpty()) return false;

			return context.Self.HasAmmo && FindBestAttackPosition(context).HasValue;
		}

		public float Score(AIContext context)
		{
			var profile = context.Archetype?.engageProfile;
			if (profile == null)
			{
				this.LogWarning($"'{context.Self.id}' has no engageProfile, returning 0");
				return 0f;
			}

			var unit = context.Self;

			// 自己血量比 ∈ [0, 1]
			float selfHpRatio = unit.maxHp > 0 ? (float)unit.CurrentHp / unit.maxHp : 0f;
			float selfHpFactor = profile.selfHpAxis.Evaluate(selfHpRatio);

			// 目标血量比 ∈ [0, 1]
			float targetHpRatio = _target.maxHp > 0 ? (float)_target.CurrentHp / _target.maxHp : 0f;
			float targetHpFactor = profile.targetHpAxis.Evaluate(targetHpRatio);

			// 路径距离比 ∈ [0, 1]
			var bestPos = FindBestAttackPosition(context);
			int pathCost = bestPos.HasValue && bestPos.Value != unit.position
				? context.ReachableArea.GetCostTo(bestPos.Value)
				: 0;
			float maxMove = unit.moveRange * unit.maxAp;
			float distRatio = maxMove > 0 ? Mathf.Clamp01(pathCost / maxMove) : 0f;
			float distFactor = profile.pathDistanceAxis.Evaluate(distRatio);

			return profile.baseScore * selfHpFactor * targetHpFactor * distFactor;
		}

		public Queue<IAtomicAction> BuildActionSequence(AIContext context)
		{
			var queue = new Queue<IAtomicAction>();

			var bestPos = FindBestAttackPosition(context);
			if (!bestPos.HasValue) return queue;

			if (bestPos.Value == context.Self.position) // 当前位置已能攻击 → 直接攻击，不移动
			{
				context.TryEnqueueAction(new AttackAction(_target.id), ref queue);
				return queue;
			}

			context.TryEnqueueAction(new MoveAction(bestPos.Value), ref queue);
			context.TryEnqueueAction(new AttackAction(_target.id), ref queue);
			return queue;
		}

		public bool ShouldAbort(AIContext context) => _target is not { IsAlive: true };

		private Vector2Int? FindBestAttackPosition(AIContext context)
		{
			var unit = context.Self;

			if (CanAttackFrom(unit.position, context))
				return unit.position;

			Vector2Int? best = null;
			int bestCost = int.MaxValue;

			foreach (var cell in context.ReachableArea.GetStoppableCellsList())
			{
				if (cell == unit.position) continue; // 当前位置已在上面 cover
				if (!CanAttackFrom(cell, context)) continue;

				int cost = context.ReachableArea.GetCostTo(cell);
				if (cost >= bestCost) continue;
				bestCost = cost;
				best = cell;
			}
			return best;
		}

		private bool CanAttackFrom(Vector2Int cell, AIContext context)
		{
			var weapon = context.Self.CurrentWeaponContainer;
			if (weapon.IsNullOrEmpty()) return false;
			if (!context.Self.HasAmmo) return false;

			float dist = Vector2Int.Distance(cell, _target.position);
			return dist <= weapon.Logic.Range() &&
			       context.VisionCalculator.TraceRay(
				       cell, _target.position, out _,
				       visionBlockers: context.VisionService.VisionBlockingCells);
		}
	}
}
