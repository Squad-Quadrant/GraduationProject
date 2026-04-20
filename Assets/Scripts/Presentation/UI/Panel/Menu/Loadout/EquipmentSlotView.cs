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
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI nameText;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI subtitleText;   // 描述/型号

		[Title("Empty State")]
		[SerializeField, ChildGameObjectsOnly] private GameObject emptyPlaceholder;   // 未装备时显示

		[Title("Highlight")]
		[SerializeField, ChildGameObjectsOnly] private GameObject highlightOverlay;

		[Title("Detail View")]
		[SerializeField, ChildGameObjectsOnly] private GameObject detail;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailName;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine1;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine2;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine3;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine4;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailDescription;

		public ELoadoutSlotKind SlotKind { get; private set; }
		public int SlotIndex { get; private set; }

		// 暴露 RectTransform 供 Dropdown 锚定
		public RectTransform RectTransform => (RectTransform)transform;

		public void Bind(ELoadoutSlotKind slotKind, int slotIndex, EquipmentConfig config, Action onClick)
		{
			SlotKind = slotKind;
			SlotIndex = slotIndex;

			Refresh(config);

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => onClick?.Invoke());

			SetHighlight(false);
		}

		// LoadoutPanel 在装备变更后调用，刷新显示
		public void Refresh(EquipmentConfig config)
		{
			bool hasEquipment = config;

			if (iconImage)
			{
				iconImage.sprite = hasEquipment ? config.icon : null;
				iconImage.enabled = hasEquipment && config.icon;
				if (iconImage.enabled) iconImage.SetNativeSize();
			}
			if (nameText) nameText.text = hasEquipment ? config.nName : "";
			if (subtitleText) subtitleText.text = hasEquipment ? config.type : "";
			if (emptyPlaceholder) emptyPlaceholder.SetActive(!hasEquipment);

			// todo: 细节界面可以展示更多信息（如伤害、重量等），目前先简单展示描述
			if (detailName) detailName.text = hasEquipment ? config.nName : "";
		}

		private void SetHighlight(bool highlight)
		{
			if (highlightOverlay)
				highlightOverlay.SetActive(highlight);
			if (detail)
				detail.SetActive(highlight);
		}

		public void OnPointerEnter(PointerEventData eventData) => SetHighlight(true);

		public void OnPointerExit(PointerEventData eventData) => SetHighlight(false);
	}
}
