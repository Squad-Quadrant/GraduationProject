using Systems.Map;
using UnityEngine;

namespace Systems.PathFinding.TraversalRule
{
	public interface ITraversalRule
	{
		/// <summary>
		/// Check if movement from one cell to an adjacent cell is allowed.
		/// </summary>
		TraversalCheckResult CheckTraversal(
			Vector2Int from,
			Vector2Int to,
			MapData mapData,
			PathFindingOptions options);
	}
}
