using Core.Events;
using Data.Runtime.Events.UI;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Unit.Skill.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.SkillMenu
{
	[RequireComponent(typeof(Button))]
	public class SkillMenuSlot : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Image iconImage;
		[SerializeField] private TextMeshProUGUI cooldownText;

		[Title("State Visuals")]
		[SerializeField] private Color normalColor = Color.white;
		[SerializeField] private Color disabledColor = new(1f, 1f, 1f, 0.4f);

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private void Awake() => button.onClick.AddListener(OnClick);

		private void OnDestroy()
		{
			if (button) button.onClick.RemoveListener(OnClick);
		}

		public void Bind(SkillLogic skill, bool interactable)
		{
			iconImage.sprite = skill.Config.icon;
			iconImage.enabled = skill.Config.icon;

			button.interactable = interactable;
			iconImage.color = interactable ? normalColor : disabledColor;

			if (!cooldownText) return;
			bool onCooldown = skill.CurrentCooldown > 0;
			cooldownText.enabled = onCooldown;
			if (onCooldown) cooldownText.text = skill.CurrentCooldown.ToString();
		}

		private void OnClick() => EventBus.Publish(new SkillSelectedEvent());
	}
}
