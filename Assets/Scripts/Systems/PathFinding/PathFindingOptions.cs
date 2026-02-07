using System;
using Sirenix.OdinInspector;
using Systems.Unit;

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
        [ShowInInspector, ReadOnly] public readonly UnitFaction MovingUnitFaction;

        [ShowInInspector, ReadOnly] public readonly string MovingUnitId;

        [ShowInInspector, ReadOnly] public readonly bool CanCrossLowWalls;

        [ShowInInspector, ReadOnly] public readonly bool CanCrossHighWalls;

        [ShowInInspector, ReadOnly] public readonly bool IgnoreTerrainWalkability;

        public PathFindingOptions(
            bool canPassThroughAllies,
            bool enemiesBlockMovement,
            UnitFaction movingUnitFaction,
            string movingUnitId,
            bool canCrossLowWalls,
            bool canCrossHighWalls,
            bool ignoreTerrainWalkability)
        {
            CanPassThroughAllies = canPassThroughAllies;
            EnemiesBlockMovement = enemiesBlockMovement;
            MovingUnitFaction = movingUnitFaction;
            MovingUnitId = movingUnitId;
            CanCrossLowWalls = canCrossLowWalls;
            CanCrossHighWalls = canCrossHighWalls;
            IgnoreTerrainWalkability = ignoreTerrainWalkability;
        }

        public static PathFindingOptions Default => new(
            canPassThroughAllies: true,
            enemiesBlockMovement: true,
            movingUnitFaction: UnitFaction.None,
            movingUnitId: null,
            canCrossLowWalls: false,
            canCrossHighWalls: false,
            ignoreTerrainWalkability: false
        );

        // Builder-style methods for easy customization
        public PathFindingOptions WithMovingUnit(string unitId, UnitFaction faction) => new(
	        CanPassThroughAllies,
	        EnemiesBlockMovement,
	        faction,
	        unitId,
	        CanCrossLowWalls,
	        CanCrossHighWalls,
	        IgnoreTerrainWalkability);

        public override string ToString() =>
            $"[PathfindingOptions] PassAllies:{CanPassThroughAllies}, " +
            $"EnemyBlock:{EnemiesBlockMovement}, Faction:{MovingUnitFaction.ToString()}";
	}
}
