using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;

namespace Systems.PathFinding
{
	[Serializable]
	public readonly struct PathFindingOptions
	{
        [ShowInInspector, ReadOnly] public readonly bool CanPassThroughAllies;

        [ShowInInspector, ReadOnly] public readonly bool EnemiesBlockMovement;

        /// <summary>
        /// The faction/team ID of the moving unit.
        /// Used to determine which units are allies vs enemies.
        /// Null means ignore unit blocking entirely.
        /// </summary>
        [ShowInInspector, ReadOnly] public readonly EUnitFaction MovingUnitFaction;

        [ShowInInspector, ReadOnly] public readonly string MovingUnitId;

        [ShowInInspector, ReadOnly] public readonly bool CanCrossLowWalls;

        [ShowInInspector, ReadOnly] public readonly bool CanCrossHighWalls;

        [ShowInInspector, ReadOnly] public readonly bool IgnoreTerrainWalkability;

        public readonly IReadOnlyCollection<Vector2Int> VisibleCells;

        public PathFindingOptions(
            bool canPassThroughAllies,
            bool enemiesBlockMovement,
            EUnitFaction movingUnitFaction,
            string movingUnitId,
            bool canCrossLowWalls,
            bool canCrossHighWalls,
            bool ignoreTerrainWalkability,
            IReadOnlyCollection<Vector2Int> visibleCells = null)
        {
            CanPassThroughAllies = canPassThroughAllies;
            EnemiesBlockMovement = enemiesBlockMovement;
            MovingUnitFaction = movingUnitFaction;
            MovingUnitId = movingUnitId;
            CanCrossLowWalls = canCrossLowWalls;
            CanCrossHighWalls = canCrossHighWalls;
            IgnoreTerrainWalkability = ignoreTerrainWalkability;
            VisibleCells = visibleCells;
        }

        public static PathFindingOptions Default => new(
            canPassThroughAllies: true,
            enemiesBlockMovement: true,
            movingUnitFaction: EUnitFaction.None,
            movingUnitId: null,
            canCrossLowWalls: false,
            canCrossHighWalls: false,
            ignoreTerrainWalkability: false);

        // Builder-style methods for easy customization
        public PathFindingOptions WithMovingUnit(string unitId, EUnitFaction faction) => new(
	        CanPassThroughAllies,
	        EnemiesBlockMovement,
	        faction,
	        unitId,
	        CanCrossLowWalls,
	        CanCrossHighWalls,
	        IgnoreTerrainWalkability);

        public PathFindingOptions WithVisibleCells(HashSet<Vector2Int> visibleCells) => new(
			CanPassThroughAllies,
			EnemiesBlockMovement,
			MovingUnitFaction,
			MovingUnitId,
			CanCrossLowWalls,
			CanCrossHighWalls,
			IgnoreTerrainWalkability,
			visibleCells);

        public override string ToString() =>
            $"[PathfindingOptions] PassAllies:{CanPassThroughAllies}, " +
            $"EnemyBlock:{EnemiesBlockMovement}, Faction:{MovingUnitFaction.ToString()}";
	}
}
