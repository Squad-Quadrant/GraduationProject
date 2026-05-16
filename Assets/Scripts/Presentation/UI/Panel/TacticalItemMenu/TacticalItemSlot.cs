using System;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	[RequireComponent(typeof(Button))]
	public class TacticalItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Image icon;
		[SerializeField, Required] private AspectRatioFitter aspectRatioFitter;
		[SerializeField, Required] private TextMeshProUGUI remainingUses;

		[SerializeField] private Sprite normalSprite;
		[SerializeField] private Sprite hoverSprite;
		[SerializeField] private Color normalColor;
		[SerializeField] private Color hoverColor;

		public int SlotIndex { get; private set; } = -1;
		public EquipmentContainer Container { get; private set; }

		public Button Button => button;
		public Action PointerEnter;
		public Action PointerExit;

		public void Setup(int slotIndex, EquipmentContainer container, bool interactable, int remainingUseAmount)
		{
			SlotIndex = slotIndex;
			Container = container;

			icon.sprite = container.Config.icon;
			icon.enabled = container.Config.icon;
			aspectRatioFitter.aspectRatio = container.Config.icon.rect.width / container.Config.icon.rect.height;

			remainingUses.text = remainingUseAmount.ToString();

			button.interactable = interactable;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetVisuals(button.interactable);
			PointerEnter?.Invoke();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			SetVisuals(button.interactable);
			PointerExit?.Invoke();
		}

		public void SetInteractable(bool interactable)
		{
			button.interactable = interactable;
			SetVisuals(interactable);
		}

		private void SetVisuals(bool interactable)
		{
			button.image.sprite = interactable ? normalSprite : hoverSprite;
			if (remainingUses) remainingUses.color = interactable ? normalColor : hoverColor;
			icon.color = interactable ? normalColor : hoverColor;
		}
	}
}
