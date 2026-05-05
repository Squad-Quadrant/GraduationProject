using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment.Config;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Presentation.UI.Panel.Menu.Loadout
{
	// 装备选择下拉
	public class EquipmentPickerDropdown : MonoBehaviour, IPointerDownHandler
	{
		[Title("References")]
		[SerializeField, Required, AssetsOnly] private EquipmentPickerItem itemPrefab;
		[SerializeField, Required, ChildGameObjectsOnly] private RectTransform itemContainer;
		[SerializeField, Required, ChildGameObjectsOnly] private RectTransform rootRect;

		[Title("Anchoring")]
		[SerializeField, Tooltip("相对于被点击槽位的偏移（像素，屏幕空间）。X>0 在右侧弹出。")]
		private Vector2 anchorOffset = new(0f, 0f);
		[SerializeField] private float widthOffset = 10f;

		private readonly List<EquipmentPickerItem> _spawnedItems = new();
		private Action _onDismiss;
		private EquipmentDetailView _detail;

		private void Awake() => gameObject.SetActive(false);

		// 展开下拉菜单
		public void Show(
			EquipmentSlotView anchorSlot,
			IReadOnlyList<EquipmentConfig> options,
			bool allowEmpty,
			EquipmentDetailView detail,
			Action<EquipmentConfig> onSelect,
			Action onDismiss)
		{
			ClearItems();
			_onDismiss = onDismiss;
			_detail = detail;

			// "卸下"选项（放最前面）
			if (allowEmpty)
			{
				var emptyItem = Instantiate(itemPrefab, itemContainer);
				emptyItem.Bind(null, detail, () => HandleSelect(null, onSelect));
				_spawnedItems.Add(emptyItem);
			}

			foreach (var config in options)
			{
				if (!config) continue;
				var captured = config;
				var item = Instantiate(itemPrefab, itemContainer);
				item.Bind(captured, detail, () => HandleSelect(captured, onSelect));
				_spawnedItems.Add(item);
			}

			PositionToAnchor(anchorSlot);
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			if (!gameObject.activeSelf) return;

			if (_detail) _detail.Hide();

			gameObject.SetActive(false);
			ClearItems();

			_onDismiss?.Invoke();
			_onDismiss = null;
			_detail = null;
		}

		// 点击透明背景 / 任何非 item 区域 → 关闭
		public void OnPointerDown(PointerEventData eventData) => Hide();

		private void HandleSelect(EquipmentConfig selected, Action<EquipmentConfig> onSelect)
		{
			onSelect?.Invoke(selected);
			Hide();
		}

		private void ClearItems()
		{
			foreach (var item in _spawnedItems.Where(item => item))
				Destroy(item.gameObject);
			_spawnedItems.Clear();
		}

		private void PositionToAnchor(EquipmentSlotView anchor)
		{
			if (!anchor) return;

			var anchorRect = anchor.RectTransform;
			var scale = rootRect.lossyScale;
			var anchorWorldPos = anchorRect.position + new Vector3(0, -anchorRect.rect.height, 0f) * scale.y;
			var worldOffset = new Vector3(
				anchorOffset.x * scale.x,
				anchorOffset.y * scale.y,
				0f);

			rootRect.position = anchorWorldPos + worldOffset;

			float worldWidth = anchorRect.rect.width * anchorRect.lossyScale.x + widthOffset;
			float rootLocalWidth = worldWidth / Mathf.Max(rootRect.lossyScale.x, 0.0001f);
			rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rootLocalWidth);
		}
	}
}
