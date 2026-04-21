using Core.Events;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public enum ECursorInfoTarget
	{
		None,      // 不显示 (Hide)
		Cell,      // 地块状态行："状态: xx" / "物体: xx" / "无视野" / "未解锁" / "状态: 空"
		Unit,      // 单位简要信息：名字 + HP + 护甲。若 UnitIsSpottedHidden 则只显示"已发现敌人"
		Movement,  // 移动预览：AP cost + 剩余 AP + 是否可停留
		Attack,    // 攻击预览：命中率 + 目标简要信息
	}

	public readonly struct CursorInfoEvent : IEvent
	{
		public ECursorInfoTarget Target { get; }

		// Cell 为 null 时表示 Hide；其余情况代表该 tooltip 语境所在的格子
		public Vector2Int? Cell { get; }
		public Vector3 WorldPosition { get; }

		// Target == Cell
		public string CellStatusLine { get; }

		// Target == Unit
		public string UnitName { get; }
		public int UnitHp { get; }
		public int UnitMaxHp { get; }
		public int UnitDefense { get; }
		public EUnitFaction UnitFaction { get; }
		public bool UnitIsSpottedHidden { get; }

		// Target == Movement
		public int MovementApCost { get; }
		public int RemainingAp { get; }
		public bool CanStopHere { get; }

		// Target == Attack
		public int HitChance { get; }
		public string TargetName { get; }
		public int TargetHp { get; }
		public int TargetMaxHp { get; }

		private CursorInfoEvent(
			ECursorInfoTarget target, Vector2Int? cell, Vector3 worldPosition,
			string cellStatusLine = null,
			string unitName = null, int unitHp = 0, int unitMaxHp = 0, int unitDefense = 0, EUnitFaction unitFaction = default, bool unitIsSpottedHidden = false,
			int movementApCost = 0, int remainingAp = 0, bool canStopHere = false,
			int hitChance = 0, string targetName = null, int targetHp = 0, int targetMaxHp = 0)
		{
			Target = target;
			Cell = cell;
			WorldPosition = worldPosition;
			CellStatusLine = cellStatusLine;
			UnitName = unitName;
			UnitHp = unitHp;
			UnitMaxHp = unitMaxHp;
			UnitDefense = unitDefense;
			UnitFaction = unitFaction;
			UnitIsSpottedHidden = unitIsSpottedHidden;
			MovementApCost = movementApCost;
			RemainingAp = remainingAp;
			CanStopHere = canStopHere;
			HitChance = hitChance;
			TargetName = targetName;
			TargetHp = targetHp;
			TargetMaxHp = targetMaxHp;
		}

		public static CursorInfoEvent Hide() => default;

		// 地块状态行
		public static CursorInfoEvent ForCell(Vector2Int cell, Vector3 worldPos, string statusLine)
			=> new(ECursorInfoTarget.Cell, cell, worldPos,
				statusLine);

		// 有视野下 hover 到单位
		public static CursorInfoEvent ForUnit(
			Vector2Int cell, Vector3 worldPos,
			string name, int hp, int maxHp, int defense, EUnitFaction faction)
			=> new(ECursorInfoTarget.Unit, cell, worldPos,
				null,
				name, hp, maxHp, defense, faction);

		// 雾中 spotted 的敌人（不泄露具体数值）
		public static CursorInfoEvent ForSpottedHiddenEnemy(Vector2Int cell, Vector3 worldPos)
			=> new(ECursorInfoTarget.Unit, cell, worldPos,
				unitFaction: EUnitFaction.Enemy, unitIsSpottedHidden: true);

		// 移动预览
		public static CursorInfoEvent ForMovement(
			Vector2Int cell, Vector3 worldPos,
			int apCost, int remainingAp, bool canStopHere)
			=> new(ECursorInfoTarget.Movement, cell, worldPos,
				 movementApCost: apCost, remainingAp: remainingAp, canStopHere: canStopHere);

		// 攻击预览
		public static CursorInfoEvent ForAttack(
			Vector2Int cell, Vector3 worldPos,
			int hitChance, string targetName, int targetHp, int targetMaxHp)
			=> new(ECursorInfoTarget.Attack, cell, worldPos,
				hitChance: hitChance, targetName: targetName, targetHp: targetHp, targetMaxHp: targetMaxHp);

		public override string ToString()
		{
			return Target switch
			{
				ECursorInfoTarget.None     => "[CursorInfo] Hidden",
				ECursorInfoTarget.Cell     => $"[CursorInfo] Cell:{Cell} '{CellStatusLine}'",
				ECursorInfoTarget.Unit     => UnitIsSpottedHidden
					? $"[CursorInfo] SpottedHidden Cell:{Cell}"
					: $"[CursorInfo] Unit:{UnitName} HP:{UnitHp}/{UnitMaxHp} Def:{UnitDefense}",
				ECursorInfoTarget.Movement => $"[CursorInfo] Move AP:{MovementApCost} Remain:{RemainingAp} Stop:{CanStopHere}",
				ECursorInfoTarget.Attack   => $"[CursorInfo] Attack Hit:{HitChance}% Target:{TargetName}",
				_                          => "[CursorInfo] Unknown"
			};
		}
	}
}
