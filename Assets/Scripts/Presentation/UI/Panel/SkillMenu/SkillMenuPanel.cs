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

		private SkillLogic _currentSkillLogic;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			backButton.onClick.RemoveAllListeners();
			backButton.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Back)));

			confirmButton.onClick.RemoveAllListeners();
			confirmButton.onClick.AddListener(() => EventBus.Publish(new TargetConfirmEvent()));
			confirmButton.interactable = false;

			if (unit.Skill == null) return;
			_currentSkillLogic = unit.Skill;
			skillName.text = unit.Skill.Config.skillName;
			skillDescription.text = unit.Skill.Config.description;

			if (_currentSkillLogic.CanUse)
			{
				EventBus.Publish(new SkillSelectedEvent());
				confirmButton.interactable = true;
			}
			else
			{
				var text = "";
				if (unit.CurrentAp < _currentSkillLogic.Config.apCost) text += "AP不足\n";
				if (_currentSkillLogic.CurrentCooldown > 0) text += $"冷却中，剩余{_currentSkillLogic.CurrentCooldown}回合";
				cooldown.text = text;
			}
		}

		protected override void OnOpen() => EventBus.Subscribe<TargetingEvent>(OnTargeting);

		protected override void OnClose() => EventBus.Unsubscribe<TargetingEvent>(OnTargeting);

		private void OnTargeting(TargetingEvent e) => confirmButton.interactable = e.TargetCell.HasValue;
	}
}
