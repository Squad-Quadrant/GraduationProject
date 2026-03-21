using System.Collections.Generic;
using System.Linq;
using Core.Log;
using UnityEngine;

namespace Systems.PathFinding
{
	public class ReachableAreaResult
	{
		private readonly Dictionary<Vector2Int, int> _costMap;
		private readonly Dictionary<Vector2Int, Vector2Int?> _parentMap;
		private readonly HashSet<Vector2Int> _stoppableCells;
		private readonly Vector2Int _origin;

		public IReadOnlyDictionary<Vector2Int, int> CostMap => _costMap;
		public IReadOnlyCollection<Vector2Int> StoppableCells => _stoppableCells;
		public Vector2Int Origin => _origin;
		public int ReachableCount => _costMap.Count;
		public int StoppableCount => _stoppableCells.Count;

		internal ReachableAreaResult(
			Vector2Int origin,
			Dictionary<Vector2Int, int> costMap,
			Dictionary<Vector2Int, Vector2Int?> parentMap,
			HashSet<Vector2Int> stoppableCells)
		{
			_origin = origin;
			_costMap = costMap ?? new Dictionary<Vector2Int, int>();
			_parentMap = parentMap ?? new Dictionary<Vector2Int, Vector2Int?>();
			_stoppableCells = stoppableCells ?? new HashSet<Vector2Int>();
		}

		public bool CanReach(Vector2Int target) => _costMap.ContainsKey(target);

		public bool CanStopAt(Vector2Int target) => _stoppableCells.Contains(target);

		public int GetCostTo(Vector2Int target) => _costMap.GetValueOrDefault(target, -1);

		public PathResult GetPathTo(Vector2Int target)
		{
			// Not reachable at all
			if (!_costMap.TryGetValue(target, out var cost))
				return PathResult.Failure();

			// Already at origin
			if (target == _origin)
				return PathResult.AtDestination(_origin);

			// Reconstruct path by following parent pointers
			var path = new List<Vector2Int>();
			var current = target;
			int maxIterations = _costMap.Count + 1;
			int iterations = 0;

			while (current != _origin && iterations < maxIterations)
			{
				path.Add(current);

				if (!_parentMap.TryGetValue(current, out var parent) || !parent.HasValue)
				{
					// Broken parent chain - shouldn't happen
					this.LogError($"Broken parent chain at {current}");
					return PathResult.Failure();
				}

				current = parent.Value;
				iterations++;
			}
			path.Add(_origin);
			path.Reverse();
			return PathResult.Success(path, cost);
		}

		public List<Vector2Int> GetStoppableCellsList() => _stoppableCells.ToList();

		public static ReachableAreaResult Empty(Vector2Int origin) => new(
			origin,
			new Dictionary<Vector2Int, int> { { origin, 0 } },
			new Dictionary<Vector2Int, Vector2Int?> { { origin, null } },
			new HashSet<Vector2Int> { origin });

		public override string ToString() =>
			$"[ReachableArea] Origin:{_origin}, Reachable:{ReachableCount}, Stoppable:{StoppableCount}";
	}
}
