using System;
using Sirenix.OdinInspector;
using Systems.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Presentation.UI.Panel.Menu.Loadout
{
	// 单位列表的单个条目
	public class UnitListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Required, ChildGameObjectsOnly] private Button button;
		[SerializeField, Required, ChildGameObjectsOnly] private Image iconImage;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI nameText;

		[Title("Selection Visual")]
		[SerializeField, ChildGameObjectsOnly] private GameObject highlightOverlay;

		public void Bind(UnitConfig config, Action onClick)
		{
			if (iconImage)
			{
				iconImage.sprite = config.icon;
				iconImage.enabled = config.icon;
			}
			if (nameText) nameText.text = config.unitName;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => onClick?.Invoke());

			SetHighlight(false);
		}

		public void SetHighlight(bool highlight)
		{
			if (highlightOverlay)
				highlightOverlay.SetActive(highlight);
		}

		public void OnPointerEnter(PointerEventData eventData) => SetHighlight(true);

		public void OnPointerExit(PointerEventData eventData) => SetHighlight(false);
	}
}
