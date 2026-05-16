using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Buff.Config;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Config;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	public class TacticalItemDetailDisplayPanel : MonoBehaviour
	{
		[SerializeField, ChildGameObjectsOnly] private List<TacticalItemDetailLine> lines;

		public void Default()
		{
			foreach (var line in lines) line.SetDefault();
		}

		public void Show(EquipmentContainer container)
		{
			if (container.Config is not TacticalItemConfig config ||
			    container.Logic is not TacticalItemLogic logic) return;
			lines[0].SetPair("AP消耗", $"{config.apCost}");
			lines[1].SetPair("剩余次数", $"{logic.RemainingUses}");

			switch (config.kind)
			{
				case ETacticalItemKind.InstantMedpack:
					if (config.appliedBuff == BuffType.InstantAddHP)
						lines[2].SetPair("治疗量", $"{config.displayHealAmount}");
					if (config.appliedBuff == BuffType.SlowAddHP)
						lines[2].SetPair("每回合治疗量", $"{config.displayHealAmount}");
					// if (config.appliedBuff == BuffType.RemoveDebuff)
					// 	lines[3].SetPair("治疗量", "");
					break;
				case ETacticalItemKind.Grenade:
					lines[2].SetPair("范围", $"{config.throwRange}格");
					lines[3].SetPair("伤害", $"{config.directDamage}");
					break;
				case ETacticalItemKind.Burn:
				case ETacticalItemKind.Light:
				case ETacticalItemKind.Smoke:
					lines[2].SetPair("范围", $"{config.throwRange}格");
					lines[3].SetPair("持续回合", $"{config.persistTurns}回合");
					break;
				case ETacticalItemKind.TimerBomb:
					lines[2].SetPair("范围", $"{config.throwRange}格");
					lines[3].SetPair("倒计时", $"{config.persistTurns}回合");
					break;
				case ETacticalItemKind.ScoutEye:
					lines[2].SetPair("持续回合", $"{config.persistTurns}回合");
					lines[3].SetPair("视野范围", $"{config.visionReach}格");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
