using Systems.AI.Config;
using UnityEngine;

namespace Systems.AI.Behavior
{
	public static class IdleTargetSelector
	{
		public static Vector2Int? GetIdleTarget(Unit.Unit unit, AIArchetype archetype)
		{
			if (!archetype)
				return TryReturnHome(unit);

			return archetype.idleBehavior switch
			{
				EIdleBehavior.Guard  => TryReturnHome(unit),
				EIdleBehavior.Patrol => TryAdvancePatrol(unit),
				_ => TryReturnHome(unit),
			};
		}

		private static Vector2Int? TryReturnHome(Unit.Unit unit) =>
			unit.position == unit.spawnPosition ? null : unit.spawnPosition;

		private static Vector2Int? TryAdvancePatrol(Unit.Unit unit)
		{
			var waypoints = unit.patrolWaypoints;

			if (waypoints == null || waypoints.Count == 0)
				return TryReturnHome(unit);

			if (unit.patrolCursor < 0 || unit.patrolCursor >= waypoints.Count)
			{
				Debug.LogWarning($"[IdleTargetSelector] '{unit.id}' patrolCursor {unit.patrolCursor} out of range, resetting to 0");
				unit.patrolCursor = 0;
			}

			if (unit.position == waypoints[unit.patrolCursor])
				unit.patrolCursor = (unit.patrolCursor + 1) % waypoints.Count;

			var target = waypoints[unit.patrolCursor];
			return unit.position == target ? null : target;
		}
	}
}
