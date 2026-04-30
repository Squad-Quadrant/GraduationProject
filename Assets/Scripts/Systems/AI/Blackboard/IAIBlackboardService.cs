using System.Collections.Generic;
using Systems.Unit;

namespace Systems.AI.Blackboard
{
	public interface IAIBlackboardService
	{
		AIBlackboard GetBlackboard(EUnitFaction faction);

		void ReportVisibleEnemies(
			EUnitFaction faction,
			int currentTurn,
			string reporterUnitId,
			IEnumerable<Unit.Unit> visibleEnemies);

		void DismissKnownEnemy(EUnitFaction faction, string enemyUnitId);
	}
}
