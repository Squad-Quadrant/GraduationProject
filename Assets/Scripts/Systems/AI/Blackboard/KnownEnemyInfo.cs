using UnityEngine;

namespace Systems.AI.Blackboard
{
	// 黑板中"曾经被任一阵营成员看到过的敌人"的记录
	public class KnownEnemyInfo
	{
		public string EnemyUnitId { get; }
		public Vector2Int LastKnownPos { get; internal set; }
		public int LastSeenTurn { get; internal set; }
		public string LastSeenByMemberId { get; internal set; }

		internal KnownEnemyInfo(string enemyUnitId, Vector2Int lastKnownPos, int lastSeenTurn, string lastSeenByMemberId)
		{
			EnemyUnitId = enemyUnitId;
			LastKnownPos = lastKnownPos;
			LastSeenTurn = lastSeenTurn;
			LastSeenByMemberId = lastSeenByMemberId;
		}

		public override string ToString() =>
			$"[KnownEnemy] {EnemyUnitId} @ {LastKnownPos} (T{LastSeenTurn}, by {LastSeenByMemberId})";
	}
}
