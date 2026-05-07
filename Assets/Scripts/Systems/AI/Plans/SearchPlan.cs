using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Systems.AI.Actions;
using Systems.AI.Behavior;
using Systems.AI.Blackboard;
using UnityEngine;

namespace Systems.AI.Plans
{
	public class SearchPlan : ITurnPlan
	{
		private const float StalenessNormDivisor = 10f;

		private readonly KnownEnemyInfo _target;

		public string Name => $"Search({_target.EnemyUnitId} @ {_target.LastKnownPos})";

		public SearchPlan(KnownEnemyInfo target) => _target = target;

		public bool IsViable(AIContext context) =>
			IsTargetInBoard(context) && IsClosestSearcher(context);

		public float Score(AIContext context)
		{
			var profile = context.Archetype?.searchProfile;
			if (profile == null)
			{
				this.LogWarning($"'{context.Self.id}' has no searchProfile, returning 0");
				return 0f;
			}

			var unit = context.Self;

			// 距离比
			int dist = ManhattanDistance(unit.position, _target.LastKnownPos);
			float maxRelevant = unit.moveRange * unit.maxAp;
			float distRatio = maxRelevant > 0 ? Mathf.Clamp01(dist / maxRelevant) : 1f;
			float distFactor = profile.pathDistanceAxis.Evaluate(distRatio);

			// 情报新鲜度
			int turnsAgo = context.CurrentTurn - _target.LastSeenTurn;
			if (turnsAgo < 0) turnsAgo = 0;
			float stalenessRatio = Mathf.Clamp01(turnsAgo / StalenessNormDivisor);
			float stalenessFactor = profile.stalenessAxis.Evaluate(stalenessRatio);

			return profile.baseScore * distFactor * stalenessFactor;
		}

		public Queue<IAtomicAction> BuildActionSequence(AIContext context)
		{
			var queue = new Queue<IAtomicAction>();
			var unit = context.Self;
			var lastKnown = _target.LastKnownPos;

			if (unit.position == lastKnown)
			{
				queue.Enqueue(new SearchCompletionAction(_target.EnemyUnitId));
				return queue;
			}

			var stoppable = context.ReachableArea.GetStoppableCellsList();
			bool canReachThisTurn = stoppable != null && stoppable.Contains(lastKnown);

			if (canReachThisTurn)
			{
				context.TryEnqueueAction(new MoveAction(lastKnown), ref queue);
				queue.Enqueue(new SearchCompletionAction(_target.EnemyUnitId));
				return queue;
			}

			var step = PathFollowingHelper.FindStepTowards(
				unit.position, lastKnown,
				context.ReachableArea,
				context.PathFinding, context.PathOptions);

			if (step == unit.position || stoppable != null && !stoppable.Contains(step)) return queue;

			context.TryEnqueueAction(new MoveAction(step), ref queue);
			return queue;
		}

		public bool ShouldAbort(AIContext context)
		{
			return !IsTargetInBoard(context) ||
			       !IsClosestSearcher(context) ||
			       context.Enemies.Any(enemy => enemy.id == _target.EnemyUnitId);
		}

		private bool IsTargetInBoard(AIContext context)
		{
			var board = context.BlackboardService.GetBlackboard(context.Self.faction);
			return board != null && board.KnownEnemies.ContainsKey(_target.EnemyUnitId);
		}

		private bool IsClosestSearcher(AIContext context)
		{
			var self = context.Self;
			int myDist = ManhattanDistance(self.position, _target.LastKnownPos);

			foreach (var other in context.UnitService.GetAllAliveUnits())
			{
				if (other.id == self.id) continue;
				if (other.faction != self.faction) continue;

				int otherDist = ManhattanDistance(other.position, _target.LastKnownPos);

				if (otherDist < myDist) return false;
				if (otherDist == myDist && string.CompareOrdinal(other.id, self.id) < 0) return false;
			}
			return true;
		}

		private static int ManhattanDistance(Vector2Int a, Vector2Int b)
			=> Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}
}
