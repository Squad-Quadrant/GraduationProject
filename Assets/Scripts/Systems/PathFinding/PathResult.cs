using System.Collections.Generic;
using UnityEngine;

namespace Systems.PathFinding
{
	public class PathResult
	{
		public bool Found { get; }

		public IReadOnlyList<Vector2Int> Path { get; }

		public int TotalCost { get; }

		public int StepCount => Found ? Path.Count - 1 : 0;

		private PathResult(bool found, IReadOnlyList<Vector2Int> path, int totalCost)
		{
			Found = found;
			Path = path ?? new List<Vector2Int>();
			TotalCost = totalCost;
		}

		public static PathResult Success(IReadOnlyList<Vector2Int> path, int totalCost) => new(true, path, totalCost);

		public static PathResult Failure() => new(false, new List<Vector2Int>(), 0);

		public static PathResult AtDestination(Vector2Int position) => new(true, new List<Vector2Int> { position }, 0);

		public override string ToString() =>
			Found ?
				$"[PathResult] Found: {Path.Count} cells, Cost: {TotalCost}" :
				"[PathResult] No path found";
	}
}
