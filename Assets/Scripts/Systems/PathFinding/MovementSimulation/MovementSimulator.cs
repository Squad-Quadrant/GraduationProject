using System.Collections.Generic;
using System.Linq;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.PathFinding.MovementSimulation
{
	public static class MovementSimulator
	{
		private const string Tag = "[MovementSimulator]";

		public static MovementSimulationResult Simulate(
			IReadOnlyList<Vector2Int> path,
			Unit.Unit movingUnit,
			HashSet<Vector2Int> originVisible,
			IVisionService visionService,
			IUnitService unitService)
		{
			if (path == null || path.Count < 2)
			{
				Debug.LogWarning($"{Tag} Trivial or null path, skipping simulation");
				var trivialVisible = visionService.CalculateVisibleCells(path?[0] ?? movingUnit.position, movingUnit.visionRange);
				return MovementSimulationResult.Completed(path ?? new List<Vector2Int>(), trivialVisible);
			}

			var everSeen = new HashSet<Vector2Int>(originVisible); // 路径中扫过的所有可见格子

			for (int i = 1; i < path.Count; i++)
			{
				var stepVisible = visionService.CalculateVisibleCells(path[i], movingUnit.visionRange);

				var newlyRevealed = new HashSet<Vector2Int>(stepVisible);
				newlyRevealed.ExceptWith(everSeen);
				everSeen.UnionWith(stepVisible);

				var enemyDiscovered = new List<Unit.Unit>();
				foreach (var cell in newlyRevealed)
				{
					var occupant = unitService.GetUnitAtPosition(cell);
					if (occupant is { IsAlive: true } && movingUnit.IsHostile(occupant))
						enemyDiscovered.Add(occupant);
				}

				if (enemyDiscovered.Count <= 0) continue;

				int stopIndex = FindLastStoppableIndex(path, i, unitService);
				var truncated = path.Take(stopIndex + 1).ToList();
				Debug.Log($"{Tag} Interrupted at step {i}/{path.Count - 1} ({path[i]}): " +
				          $"discovered {enemyDiscovered.Count} enemy(s)");
				return MovementSimulationResult.Interrupted(truncated, enemyDiscovered, everSeen);
			}
			return MovementSimulationResult.Completed(path, everSeen);
		}

		private static int FindLastStoppableIndex(
			IReadOnlyList<Vector2Int> path,
			int fromIndex,
			IUnitService unitService)
		{
			for (int i = fromIndex; i > 0; i--)
				if (unitService.GetUnitAtPosition(path[i]) == null)
					return i;
			return 0;
		}
	}
}
