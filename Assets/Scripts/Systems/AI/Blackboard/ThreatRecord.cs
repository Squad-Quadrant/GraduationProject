using UnityEngine;

namespace Systems.AI.Blackboard
{
	// 一条"我方某成员被攻击"的事件记录
	public readonly struct ThreatRecord
	{
		public string DamagedUnitId { get; }
		public Vector2Int DamagedAtPos { get; }
		public int Turn { get; }
		public string AttackerUnitId { get; }
		public Vector2Int? AttackerPos { get; }

		public ThreatRecord(
			string damagedUnitId,
			Vector2Int damagedAtPos,
			int turn,
			string attackerUnitId,
			Vector2Int? attackerPos)
		{
			DamagedUnitId = damagedUnitId;
			DamagedAtPos = damagedAtPos;
			Turn = turn;
			AttackerUnitId = attackerUnitId;
			AttackerPos = attackerPos;
		}

		public override string ToString() =>
			$"[Threat] {DamagedUnitId} @ {DamagedAtPos} hit by {AttackerUnitId ?? "?"} (T{Turn})";
	}
}
