using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
					lines[3].SetPair("治疗量", $"{config.healAmount}");
					break;
				case ETacticalItemKind.Grenade:
					lines[3].SetPair("范围", $"{config.throwRange}格");
					lines[4].SetPair("伤害", $"{config.directDamage}");
					break;
				case ETacticalItemKind.Burn:
				case ETacticalItemKind.Light:
				case ETacticalItemKind.Smoke:
					lines[3].SetPair("范围", $"{config.throwRange}格");
					lines[4].SetPair("持续回合", $"{config.persistTurns}回合");
					break;
				case ETacticalItemKind.TimerBomb:
					lines[3].SetPair("范围", $"{config.throwRange}格");
					lines[4].SetPair("倒计时", $"{config.persistTurns}回合");
					break;
				case ETacticalItemKind.ScoutEye:
					lines[3].SetPair("持续回合", $"{config.persistTurns}回合");
					lines[4].SetPair("视野范围", $"{config.visionReach}格");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
