using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Panel.AttackPreview
{
	[RequireComponent(typeof(Button))]
	public class AttackMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Required] private Button button;
		[SerializeField, Required] private Image icon;

		[SerializeField] private Sprite normalSprite;
		[SerializeField] private Sprite hoverSprite;
		[SerializeField] private Color normalColor;
		[SerializeField] private Color hoverColor;

		[SerializeField] public string mode;
		[SerializeField] public string desc;

		public Button Button => button;

		public Action PointerEnter;
		public Action PointerExit;

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
			icon.color = interactable ? normalColor : hoverColor;
		}
	}
}
