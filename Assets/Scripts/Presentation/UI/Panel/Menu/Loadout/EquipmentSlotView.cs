using System;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu.Loadout
{
	// 单个装备槽
	public class EquipmentSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField, Required, ChildGameObjectsOnly] private Button button;
		[SerializeField, Required, ChildGameObjectsOnly] private Image iconImage;
		[SerializeField, Required, ChildGameObjectsOnly] private AspectRatioFitter aspectRatioFitter;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI nameText;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI subtitleText;   // 描述/型号

		[Title("Empty State")]
		[SerializeField, ChildGameObjectsOnly] private GameObject emptyPlaceholder;   // 未装备时显示

		[Title("Highlight")]
		[SerializeField, ChildGameObjectsOnly] private GameObject highlightOverlay;

		public ELoadoutSlotKind SlotKind { get; private set; }
		public int SlotIndex { get; private set; }

		// 暴露 RectTransform 供 Dropdown 锚定
		public RectTransform RectTransform => (RectTransform)transform;

		private EquipmentConfig _currentConfig;

		private EquipmentDetailView _detail;

		public void Bind(ELoadoutSlotKind slotKind, int slotIndex, EquipmentConfig config, EquipmentDetailView detail, Action onClick)
		{
			SlotKind = slotKind;
			SlotIndex = slotIndex;
			_detail = detail;

			Refresh(config);

			button.onClick.AddListener(() => onClick?.Invoke());

			SetHighlight(false);
		}

		// LoadoutPanel 在装备变更后调用，刷新显示
		public void Refresh(EquipmentConfig config)
		{
			_currentConfig = config;

			bool hasEquipment = config;

			if (iconImage)
			{
				iconImage.sprite = hasEquipment ? config.icon : null;
				iconImage.enabled = hasEquipment && config.icon;
				if (iconImage.enabled) aspectRatioFitter.aspectRatio = config.icon.rect.width / config.icon.rect.height;
			}
			if (nameText) nameText.text = hasEquipment ? config.nName : "";
			if (subtitleText) subtitleText.text = hasEquipment ? config.type : "";
			if (emptyPlaceholder) emptyPlaceholder.SetActive(!hasEquipment);
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

		private void OnDestroy() => button.onClick.RemoveAllListeners();

		[Button]
		private void SetAspectRatio()
		{
			if (!aspectRatioFitter || !iconImage || !iconImage.sprite) return;
			aspectRatioFitter.aspectRatio = iconImage.sprite.rect.width / iconImage.sprite.rect.height;
		}
	}
}
