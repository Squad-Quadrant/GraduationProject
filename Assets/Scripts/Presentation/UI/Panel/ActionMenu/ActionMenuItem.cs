using System;
using Data.Runtime;
using Presentation.UI.Component;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Panel.ActionMenu
{
	[RequireComponent(typeof(Button))]
	public class ActionMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
        [Title("General")]
        [SerializeField] private EActionType actionType;

        [InfoBox("Attack: 0=普通, 1=精确\n" +
                 "UseTacticalItem: 槽位 0/1/2\n" +
                 "其他: 0")]
        [SerializeField] private int payload;

		[Title("Desc")]
		[SerializeField] private string description;

		[Title("Sprite")]
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Sprite normalSprite;
		[SerializeField, Required] private Sprite hoverSprite;

		[Title("Icon")]
		[SerializeField, Required] private Image icon;
		[SerializeField, Required] private AspectRatioFitter iconAspectRatio;
		[SerializeField] private Color normalColor;
		[SerializeField] private Color hoverColor;

		[Title("Dynamic Slot Display")]
		[SerializeField] private TextMeshProUGUI usesText;

		[Title("Audio")]
		[SerializeField] private UIButtonSfx sfx;

		public Button Button => button;
		public string Description => description;
        public EActionType ActionType => actionType;
        public int Payload => payload;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

		public Action<string> OnHoverEnter;
		public Action<string> OnHoverExit;

		public void SetContent(Sprite iconSprite, string desc, int? remainingUses)
		{
			if (iconSprite) icon.sprite = iconSprite;
			if (desc != null) description = desc;
			if (usesText) usesText.text = remainingUses?.ToString() ?? "";
		}

		public void SetAudioEnabled(bool isAudioEnabled) => sfx.enabled = isAudioEnabled;

		[Button]
		public void SetIconAspectRatio()
		{
			if (iconAspectRatio && icon && icon.sprite)
				iconAspectRatio.aspectRatio = icon.sprite.rect.width / icon.sprite.rect.height;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			button.image.sprite = button.interactable ? hoverSprite : normalSprite;
			icon.color = button.interactable ? hoverColor : normalColor;
			if (usesText.isActiveAndEnabled) usesText.color = button.interactable ? hoverColor : normalColor;
			OnHoverEnter?.Invoke(Description);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			button.image.sprite = normalSprite;
			icon.color = normalColor;
			if (usesText.isActiveAndEnabled) usesText.color = normalColor;
			OnHoverExit?.Invoke(description);
		}

        public void SetActive(bool enable) => gameObject.SetActive(enable);
	}
}
