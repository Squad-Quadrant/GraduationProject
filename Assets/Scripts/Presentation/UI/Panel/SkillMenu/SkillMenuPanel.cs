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
		[SerializeField, Required, ChildGameObjectsOnly] private Button confirmButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button backButton;

		private Systems.Unit.Unit _unit;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			_unit = unit;

			confirmButton.interactable = false;

			if (unit.Skill == null) return;
			skillName.text = unit.Skill.Config.skillName;
			skillDescription.text = unit.Skill.Config.description;
		}

		protected override void OnOpen()
		{
			EventBus.Subscribe<TargetingEvent>(OnTargeting);

			if (_unit == null) return;
			var currentSkillLogic = _unit.Skill;

			if (currentSkillLogic == null)
			{
				cooldown.text = "角色无技能";
				return;
			}

			if (currentSkillLogic.CanUse)
				EventBus.Publish(new SkillSelectedEvent());
			else
			{
				var text = "";
				if (_unit.CurrentAp < currentSkillLogic.Config.apCost) text += "AP不足\n";
				if (currentSkillLogic.CurrentCooldown > 0) text += $"冷却中，剩余{currentSkillLogic.CurrentCooldown}回合";
				cooldown.text = text;
			}

			confirmButton.onClick.AddListener(() => EventBus.Publish(new TargetConfirmEvent()));
			backButton.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Back)));
		}

		protected override void OnClose()
		{
			EventBus.Unsubscribe<TargetingEvent>(OnTargeting);

			backButton.onClick.RemoveAllListeners();
			confirmButton.onClick.RemoveAllListeners();
		}

		private void OnTargeting(TargetingEvent e) => confirmButton.interactable = e.TargetCell.HasValue;
	}
}
