using Data.Config.Map;
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
			if (!mapData.IsInBounds(to))
				return TraversalCheckResult.Blocked;

			var targetCell = mapData.GetCell(to);
			if (targetCell == null ||
			    (!options.IgnoreTerrainWalkability && !targetCell.IsWalkable) ||
			    !CanCrossWall(from, to, mapData, options))
				return TraversalCheckResult.Blocked;

			if (targetCell.SceneActor != null) // todo: check scene actor, now all scene actors are blocking; unit belongs to scene actor?
				return TraversalCheckResult.Blocked;

			int baseCost = options.IgnoreTerrainWalkability ? 1 : targetCell.MoveCost;

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

		private bool CanCrossWall(Vector2Int from, Vector2Int to, MapData mapData, PathFindingOptions options)
		{
			var wall = mapData.GetWall(new WallKey(from, to));
			if (wall == null)
				return true;
			return wall.WallType switch
			{
				WallType.None => true,
				WallType.LowWall => options.CanCrossLowWalls,
				WallType.HighWall => options.CanCrossHighWalls,
				_ => false
			};
		}

		private OccupationType CheckUnitOccupation(Vector2Int position, PathFindingOptions options)
		{
			if (_unitService == null)
				return OccupationType.Empty;

			var units = _unitService.GetUnitsWhere(u => u.position == position);
			if (units.Count == 0)
				return OccupationType.Empty;

			var occupant = units[0];

			if (!string.IsNullOrEmpty(options.MovingUnitId) && occupant.id == options.MovingUnitId)
				return OccupationType.Self;

			if (options.MovingUnitFaction == EUnitFaction.None)
				return OccupationType.Enemy;

			bool isAlly = occupant.faction == options.MovingUnitFaction;

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
