using UnityEngine;

namespace Systems.PathFinding
{
	public interface IPathFindingService
	{
		ReachableAreaResult GetReachableArea(Vector2Int origin, int maxMovementPoints, PathFindingOptions options);

		/// <summary>
		/// Internally uses GetReachableArea. If you need both area and paths, call GetReachableArea once and use its GetPathTo method for efficiency.
		/// </summary>
		PathResult FindPath(Vector2Int from, Vector2Int to, PathFindingOptions options);

		/// <summary>
		/// Quick check if a direct path exists between two adjacent cells.
		/// </summary>
		bool CanTraverseBetween(Vector2Int from, Vector2Int to, PathFindingOptions options);
	}
}
