using Core.Events;
using Data.Runtime.Events.UI;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	[RequireComponent(typeof(Button))]
	public class TacticalItemSlot : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Image iconImage;

		[Title("State Visuals")]
		[SerializeField] private Color normalColor = Color.white;
		[SerializeField] private Color disabledColor = new(1f, 1f, 1f, 0.4f);

		private int _slotIndex = -1; // 运行时由 Panel 注入

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private void Awake() => button.onClick.AddListener(OnClick);

		private void OnDestroy()
		{
			if (button) button.onClick.RemoveListener(OnClick);
		}

		public void Bind(int slotIndex, EquipmentContainer container, bool interactable)
		{
			_slotIndex = slotIndex;

			iconImage.sprite = container.Config.icon;
			iconImage.enabled = container.Config.icon;

			button.interactable = interactable;
			iconImage.color = interactable ? normalColor : disabledColor;
		}

		private void OnClick()
		{
			if (_slotIndex < 0)
			{
				Debug.LogError("[TacticalItemSlot] Clicked before Bind(). slotIndex is invalid.");
				return;
			}
			EventBus.Publish(new TacticalItemSelectedEvent(_slotIndex));
		}
	}
}
