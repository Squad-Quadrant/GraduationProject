using Systems.Map;
using Systems.Unit;
using UnityEngine;

namespace Systems.PathFinding.TraversalRule
{
	public class DefaultTraversalRule : ITraversalRule
	{
		private readonly IUnitService _unitService;

		public DefaultTraversalRule(IUnitService unitService) => _unitService = unitService;

		public TraversalCheckResult CheckTraversal(Vector2Int from, Vector2Int to, MapData mapData, PathFindingOptions options)
		{
			throw new System.NotImplementedException();
		}
	}
}
