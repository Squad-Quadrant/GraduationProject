using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Systems.Map;
using Systems.PathFinding.TraversalRule;
using UnityEngine;

namespace Systems.PathFinding
{
	public class PathFindingService : IPathFindingService
	{
		private readonly IMapService _mapService;
		private readonly ITraversalRule _traversalRule;

		public PathFindingService(IMapService mapService, ITraversalRule traversalRule)
		{
			_mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
			_traversalRule = traversalRule ?? throw new ArgumentNullException(nameof(traversalRule));
			this.Log($"Initialized with rule: {traversalRule.GetType().Name}");
		}

		public ReachableAreaResult GetReachableArea(Vector2Int origin, int maxMovementPoints, PathFindingOptions options)
		{
			var mapData = _mapService.Data;
			if (!mapData.IsInBounds(origin))
			{
				this.LogWarning($"Origin {origin} is out of bounds");
				return ReachableAreaResult.Empty(origin);
			}

			var costMap = new Dictionary<Vector2Int, int>();
			var parentMap = new Dictionary<Vector2Int, Vector2Int?>();
			var stoppableCells = new HashSet<Vector2Int>();
			var frontier = new List<Vector2Int> { origin };

			costMap[origin] = 0;
			parentMap[origin] = null;
			stoppableCells.Add(origin);

			while (frontier.Count > 0)
			{
				// Find cell with minimum cost
				int minIndex = 0;
				int minCost = costMap[frontier[0]];
				for (int i = 1; i < frontier.Count; i++)
				{
					int cost = costMap[frontier[i]];
					if (cost >= minCost) continue;
					minCost = cost;
					minIndex = i;
				}

				var current = frontier[minIndex];
				frontier.RemoveAt(minIndex);

				var neighbors = mapData.GetNeighbors(current);
				foreach (var neighbor in neighbors.Select(neighborCell => neighborCell.Position))
				{
					var traversal = _traversalRule.CheckTraversal(current, neighbor, mapData, options);
					if (!traversal.CanPass) continue;

					int newCost = minCost + traversal.Cost;
					if (newCost > maxMovementPoints) continue;
					if (costMap.TryGetValue(neighbor, out var existingCost) && newCost >= existingCost) continue;

					costMap[neighbor] = newCost;
					parentMap[neighbor] = current;

					if (!frontier.Contains(neighbor))
						frontier.Add(neighbor);

					if (traversal.CanStop)
						stoppableCells.Add(neighbor);
				}
			}
			var result = new ReachableAreaResult(origin, costMap, parentMap, stoppableCells);
			this.Log($"Calculated: {result}");
			return result;
		}

		public PathResult FindPath(Vector2Int from, Vector2Int to, PathFindingOptions options)
		{
			if (from == to)
				return PathResult.AtDestination(from);
			var area = GetReachableArea(from, int.MaxValue, options);
			return area.GetPathTo(to);
		}

		public bool CanTraverseBetween(Vector2Int from, Vector2Int to, PathFindingOptions options)
		{
			var delta = to - from;
			if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1)
				return false;
			var traversal = _traversalRule.CheckTraversal(from, to, _mapService.Data, options);
			return traversal.CanPass;
		}
	}
}
