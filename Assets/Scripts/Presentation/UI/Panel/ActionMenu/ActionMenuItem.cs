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

		[Title("Disable Dimming")]
		[SerializeField, Required] private CanvasGroup canvasGroup;
		[SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

		[Title("Audio")]
		[SerializeField] private UIButtonSfx sfx;

		public Button Button => button;
		public string Description => description;
        public EActionType ActionType => actionType;
        public int Payload => payload;

        private bool _isHovered;

        public bool Interactable
        {
	        get => button.interactable;
	        set
	        {
		        button.interactable = value;
		        if (!value) _isHovered = false;
		        RefreshVisuals();
	        }
        }

        public Action<string> OnHoverEnter;
		public Action<string> OnHoverExit;

		private void OnEnable() => RefreshVisuals();

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

		private void RefreshVisuals()
		{
			bool interactable = button.interactable;
			canvasGroup.alpha = interactable ? 1f : disabledAlpha;

			bool showHover = interactable && _isHovered;
			button.image.sprite = showHover ? hoverSprite : normalSprite;

			Color c = showHover ? hoverColor : normalColor;
			icon.color = c;
			if (usesText) usesText.color = c;
		}


		public void OnPointerEnter(PointerEventData eventData)
		{
			OnHoverEnter?.Invoke(Description);
			if (!button.interactable) return;
			_isHovered = true;
			RefreshVisuals();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			OnHoverExit?.Invoke(Description);
			if (!_isHovered) return;
			_isHovered = false;
			RefreshVisuals();
		}

        public void SetActive(bool enable) => gameObject.SetActive(enable);
	}
}
