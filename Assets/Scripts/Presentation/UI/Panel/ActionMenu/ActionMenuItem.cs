using System;
using Data.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Component
{
	[RequireComponent(typeof(Button))]
	public class ActionMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
        [Title("General")]
        [SerializeField] private EActionType actionType;
		[Title("Desc")]
		[SerializeField] private string description;

		[Title("Sprite")]
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Sprite normalSprite;
		[SerializeField, Required] private Sprite hoverSprite;

		[Title("Icon")]
		[SerializeField, Required] private Image icon;
		[SerializeField] private Color normalColor;
		[SerializeField] private Color hoverColor;

		public Button Button => button;
		public string Description => description;
        public EActionType ActionType => actionType;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

		public event Action<string> OnHoverEnter;
		public event Action<string> OnHoverExit;

		public void OnPointerEnter(PointerEventData eventData)
		{
			button.image.sprite = button.interactable ? hoverSprite : normalSprite;
			icon.color = button.interactable ? hoverColor : normalColor;
			OnHoverEnter?.Invoke(Description);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			button.image.sprite = normalSprite;
			icon.color = normalColor;
			OnHoverExit?.Invoke(description);
		}

        public void Switch(bool enable)
        {
            gameObject.SetActive(enable);
        }
	}
}
