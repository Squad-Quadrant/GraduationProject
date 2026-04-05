using System.Collections.Generic;
using Systems.AI.Config;
using Systems.PathFinding;
using UnityEngine;

namespace Systems.AI
{
	public class AIContext
	{
		public Unit.Unit Self { get; }
		public AIBrainConfig Brain { get; }
		public List<Unit.Unit> Enemies { get; }
		public List<Unit.Unit> Allies { get; }
		public ReachableAreaResult ReachableArea { get; }
		public HashSet<Vector2Int> VisibleCells { get; }
        // public List<Unit.Unit> AttackableEnemies { get; }

		public AIContext(
			Unit.Unit self,
			List<Unit.Unit> enemies,
			List<Unit.Unit> allies,
			ReachableAreaResult reachableArea,
			HashSet<Vector2Int> visibleCells)
		{
			Self = self;
			Brain = self.aiBrainConfig;
			Enemies = enemies;
			Allies = allies;
			ReachableArea = reachableArea;
			VisibleCells = visibleCells;
            // AttackableEnemies = attackableEnemies;
		}
	}
}
