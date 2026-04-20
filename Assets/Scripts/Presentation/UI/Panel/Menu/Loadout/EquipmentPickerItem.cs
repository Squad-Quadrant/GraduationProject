using System;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu.Loadout
{
	// 下拉菜单中的单个装备条目
	public class EquipmentPickerItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Required, ChildGameObjectsOnly] private Button button;
		[SerializeField, Required, ChildGameObjectsOnly] private Image iconImage;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI nameText;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI classText;

		[Title("Empty Slot Option")]
		[SerializeField, Tooltip("config==null 时显示的文本（如\"空\"、\"不装备\"）")]
		private string emptySlotLabel = "空";

		[Title("Highlight")]
		[SerializeField, ChildGameObjectsOnly] private GameObject highlightOverlay;

		private EquipmentConfig _currentConfig;
		private EquipmentDetailView _detail;

		// config 为 null 时表示"卸下装备"
		public void Bind(EquipmentConfig config, EquipmentDetailView detail, Action onClick)
		{
			_currentConfig = config;
			_detail = detail;

			if (config)
			{
				if (iconImage)
				{
					iconImage.sprite = config.icon;
					iconImage.enabled = config.icon;
					if (iconImage.enabled) iconImage.SetNativeSize();
				}
				if (nameText) nameText.text = config.nName;
				if (classText) classText.text = config.type;
			}
			else
			{
				if (iconImage) iconImage.enabled = false;
				if (nameText) nameText.text = emptySlotLabel;
				if (classText) classText.text = "";
			}

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => onClick?.Invoke());

			SetHighlight(false);
		}

		private void SetHighlight(bool highlight)
		{
			if (highlightOverlay)
				highlightOverlay.SetActive(highlight);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			SetHighlight(true);
			if (_detail && _currentConfig)
				_detail.Show(_currentConfig);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			SetHighlight(false);
			if (_detail)
				_detail.Hide();
		}

		private void OnDisable()
		{
			SetHighlight(false);
			if (_detail) _detail.Hide();
		}
	}
}
