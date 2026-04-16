using System.Linq;
using Systems.Map;
using Systems.Map.Config;
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
			if (!mapData.IsInBounds(to))
				return TraversalCheckResult.Blocked;

			var toCell = mapData.GetCell(to);
			if (toCell == null ||
			    (!options.IgnoreTerrainWalkability && !toCell.IsWalkable) ||
			    !CanCrossWall(from, to, mapData, options))
				return TraversalCheckResult.Blocked;

			var direction = to - from;
			var fromCell = mapData.GetCell(from);
			if (fromCell.SceneActor != null && fromCell.SceneActor.BlockMovement.Contains(direction) ||
			    toCell.SceneActor != null && toCell.SceneActor.BlockMovement.Contains(-direction))
				return TraversalCheckResult.Blocked;

			int baseCost = options.IgnoreTerrainWalkability ? 1 : toCell.MoveCost;

			var occupationResult = CheckUnitOccupation(to, options);
			return occupationResult switch
			{
				OccupationType.Empty => TraversalCheckResult.Stoppable(baseCost),
				OccupationType.Ally => TraversalCheckResult.Passable(baseCost),
				OccupationType.Enemy => TraversalCheckResult.Blocked,
				OccupationType.Self => TraversalCheckResult.Stoppable(baseCost),
				_ => TraversalCheckResult.Blocked
			};
		}

		private static bool CanCrossWall(Vector2Int from, Vector2Int to, MapData mapData, PathFindingOptions options)
		{
			var wall = mapData.GetWall(new WallKey(from, to));
			if (wall == null)
				return true;
			return wall.Type switch
			{
				WallType.None => true,
				WallType.LowWall => options.CanCrossLowWalls,
				WallType.HighWall => options.CanCrossHighWalls,
				_ => false
			};
		}

		private OccupationType CheckUnitOccupation(Vector2Int position, PathFindingOptions options)
		{
			var unit = _unitService.GetUnitAtPosition(position);
			if (unit == null) // 没单位
				return OccupationType.Empty;

			if (!string.IsNullOrEmpty(options.MovingUnitId) && unit.id == options.MovingUnitId) // 是自己
				return OccupationType.Self;

			if (options.VisibleCells != null && !options.VisibleCells.Contains(position)) // 看不见 = 没有
				return OccupationType.Empty;

			// 阵营判断
			if (options.MovingUnitFaction == EUnitFaction.None)
				return OccupationType.Enemy;
			bool isAlly = unit.faction == options.MovingUnitFaction;
			if (isAlly)
				return options.CanPassThroughAllies ? OccupationType.Ally : OccupationType.Enemy;
			return options.EnemiesBlockMovement ? OccupationType.Enemy : OccupationType.Empty;
		}

		private enum OccupationType
		{
			Empty,  // No unit
			Self,   // The moving unit itself
			Ally,   // Same faction, can pass through
			Enemy   // Different faction, blocks movement
		}
	}
}
