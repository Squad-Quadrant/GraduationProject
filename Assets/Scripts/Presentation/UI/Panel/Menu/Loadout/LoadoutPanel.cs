using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data;
using Data.Config;
using Presentation.UI.Component.UnitPortrait;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit;
using Systems.Unit.Equipment.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu.Loadout
{
	public struct LoadoutPanelData
	{
		public LevelConfig Level;                  // 当前选中的关卡
		public DataManager DataManager;            // 装备/配装查询 + 写入
		public Action OnStartBattle;               // 点"开始行动"的回调
		public Action OnBack;                      // 点"返回"的回调
	}

	public class LoadoutPanel : UIPanel, IInitializable<LoadoutPanelData>
	{
		[Title("角色选择")]
		[SerializeField, Required, ChildGameObjectsOnly] private Transform unitListRoot;
		[SerializeField, Required, AssetsOnly] private UnitListItem unitListItemPrefab;

		[Title("角色信息")]
		[SerializeField, Required, ChildGameObjectsOnly] private Transform portraitContainer;
		[SerializeField, Required, AssetsOnly] private UnitPortraitView defaultPortraitPrefab;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI unitNameText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI unitClassText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI unitDescriptionText;
		[SerializeField, Required, ChildGameObjectsOnly] private Slider hpSlider;
		[SerializeField, ChildGameObjectsOnly] private Slider defenseSlider;
		[SerializeField, ChildGameObjectsOnly] private Slider moveRangeSlider;
		[SerializeField, ChildGameObjectsOnly] private Slider speedSlider;
		[SerializeField, ChildGameObjectsOnly] private Slider visionSlider;
		[SerializeField, ChildGameObjectsOnly] private Transform apRoot;
		[SerializeField, AssetsOnly] private GameObject apPrefab;
		[SerializeField, ChildGameObjectsOnly] private Button leftButton;
		[SerializeField, ChildGameObjectsOnly] private Button rightButton;

		[Title("装备槽")]
		[SerializeField, Required, ChildGameObjectsOnly] private EquipmentSlotView mainWeaponSlot;
		[SerializeField, Required, ChildGameObjectsOnly] private EquipmentSlotView secondaryWeaponSlot;
		[SerializeField, Required, ChildGameObjectsOnly]
		[ListDrawerSettings(ShowFoldout = false)]
		private List<EquipmentSlotView> tacticalItemSlots;   // 必须 3 个

		[Title("装备选择下拉菜单")]
		[SerializeField, Required, ChildGameObjectsOnly] private EquipmentPickerDropdown dropdown;
		[SerializeField, Required, ChildGameObjectsOnly] private EquipmentDetailView equipmentDetailView;

		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly] private Button startBattleButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button backButton;

		[Title("归一化最大值")]
		[SerializeField, Tooltip("HP slider 的分母，超过此值的单位 HP 也会显示为满")]
		private int hpScaleMax = 200;
		[SerializeField] private int defenseScaleMax = 50;
		[SerializeField] private int moveRangeScaleMax = 10;
		[SerializeField] private int speedScaleMax = 20;
		[SerializeField] private int visionScaleMax = 10;

		private LoadoutPanelData _data;
		private readonly List<UnitListItem> _spawnedUnitItems = new();

		// configId → UnitListItem，切换高亮用
		// private readonly Dictionary<string, UnitListItem> _unitItemByConfigId = new();

		private UnitConfig _currentUnit;

		public void DataInitialize(LoadoutPanelData data)
		{
			_data = data;

			BuildUnitList();
			WireButtons();
			RefreshStartButtonState();

			// 默认选中第一个单位
			var units = data.DataManager.GetPlayerUnitConfigs(data.Level);
			if (units.Count > 0)
				SelectUnit(units[0]);

			leftButton.onClick.AddListener(() =>
			{
				if (units.Count == 0) return;
				if (!_currentUnit)
				{
					SelectUnit(units[0]);
					return;
				}

				int currentIndex = 0;
				for (int i = 0; i < units.Count; i++)
				{
					if (units[i].configId != _currentUnit.configId) continue;
					currentIndex = i;
					break;
				}
				int prevIndex = (currentIndex - 1) < 0 ? units.Count - 1 : currentIndex - 1;
				SelectUnit(units[prevIndex]);
			});

			rightButton.onClick.AddListener(() =>
			{
				if (units.Count == 0) return;
				if (!_currentUnit)
				{
					SelectUnit(units[0]);
					return;
				}

				int currentIndex = 0;
				for (int i = 0; i < units.Count; i++)
				{
					if (units[i].configId != _currentUnit.configId) continue;
					currentIndex = i;
					break;
				}
				int nextIndex = (currentIndex + 1) >= units.Count ? 0 : currentIndex + 1;
				SelectUnit(units[nextIndex]);
			});
		}

		protected override void OnClose()
		{
			// 关闭下拉，清左栏动态条目
			if (dropdown) dropdown.Hide();
			ClearUnitList();
			_currentUnit = null;

			if (!_currentPortrait) return;
			Destroy(_currentPortrait.gameObject);
			_currentPortrait = null;

			leftButton.onClick.RemoveAllListeners();
			rightButton.onClick.RemoveAllListeners();
			startBattleButton.onClick.RemoveAllListeners();
			backButton.onClick.RemoveAllListeners();
		}

		#region 单位选择

		private void BuildUnitList()
		{
			ClearUnitList();

			var units = _data.DataManager.GetPlayerUnitConfigs(_data.Level);
			foreach (var unit in units)
			{
				if (!unit) continue;
				var captured = unit;

				var item = Instantiate(unitListItemPrefab, unitListRoot);
				item.Bind(captured, () => SelectUnit(captured));
				_spawnedUnitItems.Add(item);
				// _unitItemByConfigId[captured.configId] = item;
			}
		}

		private void ClearUnitList()
		{
			foreach (var item in _spawnedUnitItems.Where(item => item))
				Destroy(item.gameObject);
			_spawnedUnitItems.Clear();
			// _unitItemByConfigId.Clear();
		}

		private void SelectUnit(UnitConfig unit)
		{
			if (!unit) return;

			if (dropdown) dropdown.Hide(); // 切单位时自动关下拉

			_currentUnit = unit;

			// // 更新左栏高亮
			// foreach (var (configId, item) in _unitItemByConfigId)
			// {
			// 	if (!item) continue;
			// 	item.SetHighlight(configId == unit.configId);
			// }

			RefreshUnitDetail(unit);
			RefreshEquipmentSlots(unit);
		}

		private void RefreshUnitDetail(UnitConfig unit)
		{
			RefreshPortrait(unit);

			if (unitNameText) unitNameText.text = unit.unitName;
			if (unitClassText)
			{
				bool hasClass = !string.IsNullOrEmpty(unit.unitClass);
				unitClassText.gameObject.SetActive(hasClass);
				if (hasClass) unitClassText.text = unit.unitClass;
			}
			if (unitDescriptionText) unitDescriptionText.text = unit.description;

			SetSliderValue(hpSlider, unit.maxHp, hpScaleMax);
			SetSliderValue(defenseSlider, unit.defense, defenseScaleMax);
			SetSliderValue(moveRangeSlider, unit.moveRange, moveRangeScaleMax);
			SetSliderValue(speedSlider, unit.speed, speedScaleMax);
			SetSliderValue(visionSlider, unit.visionRange, visionScaleMax);

			for (int i = apRoot.childCount - 1; i >= 0; i--)
				Destroy(apRoot.GetChild(i).gameObject);
			for (int i = 0; i < unit.actionPoints; i++)
				Instantiate(apPrefab, apRoot);
		}

		private UnitPortraitView _currentPortrait;

		private void RefreshPortrait(UnitConfig unit)
		{
			if (_currentPortrait) Destroy(_currentPortrait.gameObject);

			var prefab = unit.portraitPrefabLoadout ? unit.portraitPrefabLoadout : defaultPortraitPrefab;
			if (!prefab) return; // 连占位符也没挂时跳过,不抛错

			_currentPortrait = Instantiate(prefab, portraitContainer);
			_currentPortrait.Init();
		}

		private static void SetSliderValue(Slider slider, int value, int scaleMax)
		{
			if (!slider) return;
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.value = Mathf.Clamp01((float)value / Mathf.Max(1, scaleMax));
		}

		#endregion

		#region Equipment Slots

		private void RefreshEquipmentSlots(UnitConfig unit)
		{
			var loadout = _data.DataManager.GetPlayerLoadoutForEditing(unit, _data.Level);
			if (loadout == null)
			{
				this.LogError($"No loadout for unit '{unit.configId}'");
				return;
			}

			// 主武器
			var mainCfg = _data.DataManager.GetEquipment(loadout.mainWeaponId);
			mainWeaponSlot.Bind(
				ELoadoutSlotKind.MainWeapon,
				slotIndex: 0,
				mainCfg,
				equipmentDetailView,
				onClick: () => OpenDropdownForSlot(mainWeaponSlot));

			// 副武器
			var secondaryCfg = _data.DataManager.GetEquipment(loadout.secondaryWeaponId);
			secondaryWeaponSlot.Bind(
				ELoadoutSlotKind.SecondaryWeapon,
				slotIndex: 0,
				secondaryCfg,
				equipmentDetailView,
				onClick: () => OpenDropdownForSlot(secondaryWeaponSlot));

			// 道具槽 x3
			loadout.NormalizeTacticalSlots();
			for (int i = 0; i < tacticalItemSlots.Count && i < Data.Config.Loadout.TacticalItemSlotCount; i++)
			{
				int captured = i;
				var slot = tacticalItemSlots[i];
				if (!slot) continue;

				var itemCfg = _data.DataManager.GetEquipment(loadout.tacticalItemIds[i]);
				slot.Bind(
					ELoadoutSlotKind.TacticalItem,
					slotIndex: captured,
					itemCfg,
					equipmentDetailView,
					onClick: () => OpenDropdownForSlot(slot));
			}
		}

		#endregion

		#region 下拉菜单

		private void OpenDropdownForSlot(EquipmentSlotView slot)
		{
			if (!_currentUnit) return;

			var options = _data.DataManager.GetEquipmentsForSlot(slot.SlotKind);

			// 主武器强制不能空；副武器和道具允许空
			bool allowEmpty = slot.SlotKind != ELoadoutSlotKind.MainWeapon;

			dropdown.Show(
				anchorSlot: slot,
				options: options,
				allowEmpty: allowEmpty,
				equipmentDetailView,
				onSelect: selected => ApplyEquipmentChange(slot, selected),
				onDismiss: null);
		}

		// 写入 PlayerLoadouts + 刷新对应槽位 UI + 刷新开始按钮状态
		private void ApplyEquipmentChange(EquipmentSlotView slot, EquipmentConfig selected)
		{
			if (!_currentUnit) return;

			var loadout = _data.DataManager.GetPlayerLoadoutForEditing(_currentUnit, _data.Level);
			if (loadout == null) return;

			int newId = selected ? selected.id : 0;

			switch (slot.SlotKind)
			{
				case ELoadoutSlotKind.MainWeapon:
					loadout.mainWeaponId = newId;
					break;

				case ELoadoutSlotKind.SecondaryWeapon:
					loadout.secondaryWeaponId = newId;
					break;

				case ELoadoutSlotKind.TacticalItem:
					loadout.NormalizeTacticalSlots();
					if (slot.SlotIndex >= 0 && slot.SlotIndex < loadout.tacticalItemIds.Length)
						loadout.tacticalItemIds[slot.SlotIndex] = newId;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			slot.Refresh(selected);   // 立即更新槽位显示，无需整个 Refresh
			RefreshStartButtonState();

			this.Log($"Equipped {(selected ? selected.displayName : "<empty>")} to {_currentUnit.configId}/{slot.SlotKind}[{slot.SlotIndex}]");
		}

		#endregion

		#region 开始游戏

		private void WireButtons()
		{
			startBattleButton.onClick.AddListener(() => _data.OnStartBattle?.Invoke());
			backButton.onClick.AddListener(() => _data.OnBack?.Invoke());
		}

		// 所有 Player 单位都必须有主武器
		private void RefreshStartButtonState()
		{
			var units = _data.DataManager.GetPlayerUnitConfigs(_data.Level);
			bool allHaveMainWeapon = true;

			foreach (var unit in units)
			{
				var loadout = _data.DataManager.GetPlayerLoadoutForEditing(unit, _data.Level);
				if (loadout != null && loadout.mainWeaponId > 0) continue;

				allHaveMainWeapon = false;
				break;
			}

			startBattleButton.interactable = allHaveMainWeapon;
		}

		#endregion
	}
}
