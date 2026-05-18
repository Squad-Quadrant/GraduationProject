using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit.Skill.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.SkillMenu
{
	public class SkillMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI skillName;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI skillDescription;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI cooldown;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			var skill = unit.Skill;
			if (skill == null)
			{
				skillName.text = "";
				skillDescription.text = "";
				cooldown.text = "角色无技能";
				return;
			}

			skillName.text = skill.Config.skillName;
			skillDescription.text = skill.Config.description;

			if (skill.CanUse)
			{
				cooldown.text = "";
				return;
			}

			var sb = new System.Text.StringBuilder();
			if (unit.CurrentAp < skill.Config.apCost) sb.AppendLine("AP不足");
			if (skill.CurrentCooldown > 0) sb.Append($"冷却中，剩余{skill.CurrentCooldown}回合");
			cooldown.text = sb.ToString();
		}
	}
}
