using Systems.PathFinding;
using UnityEngine;

namespace Systems.AI.Behavior
{
	public static class PathFollowingHelper
	{
		public static Vector2Int FindStepTowards(
			Vector2Int origin,
			Vector2Int target,
			ReachableAreaResult reachable,
			IPathFindingService pathFinding,
			PathFindingOptions options)
		{
			if (origin == target) return origin;

			var path = pathFinding.FindPath(origin, target, options);
			if (!path.Found || path.Path is not { Count: > 1 })
				return origin;

			for (int i = path.Path.Count - 1; i >= 1; i--)
			{
				var cell = path.Path[i];
				if (reachable.CanStopAt(cell)) return cell;
			}

			return origin;
		}
	}
}
