using Core.Events;
using Data.Runtime.Events.Unit;
using Presentation.UI.Component.UnitPortrait;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public class UnitInfoPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[TitleGroup("References")]
		[SerializeField, Required, ChildGameObjectsOnly] private Transform portraitContainer;
		[SerializeField, Required] private UnitPortraitView defaultPortraitPrefab;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI unitNameText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI unitClassText;
		[SerializeField, Required, ChildGameObjectsOnly] private Slider hpSlider;
		[SerializeField, Required, ChildGameObjectsOnly] private Slider defenseSlider;
		[SerializeField, Required, ChildGameObjectsOnly] private Transform weaponRoot;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI weaponText;
		[SerializeField, Required, ChildGameObjectsOnly] private Image weaponIcon;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI remainingAmmoText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI maxAmmoText;
		[SerializeField, Required, ChildGameObjectsOnly] private Transform apRoot;
		[SerializeField, AssetsOnly] private GameObject apPrefab;

        private Systems.Unit.Unit _currentUnit;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			if (unit == null) return;
            _currentUnit = unit;
			Refresh(unit);
		}

		protected override void OnOpen() => EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitAttacked);
		protected override void OnClose()
		{
			EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitAttacked);
			if (!_currentPortrait) return;
			Destroy(_currentPortrait.gameObject);
			_currentPortrait = null;
		}

		private void OnUnitAttacked(UnitInfoChangedEvent e)
        {
            if (e.Unit != _currentUnit) return;
            Refresh(e.Unit);
        }

        public void Refresh(Systems.Unit.Unit unit)
		{
			RefreshPortrait(unit);
			unitNameText.text = unit.name;
			bool hasClass = !string.IsNullOrEmpty(unit.unitClass);
			unitClassText.gameObject.SetActive(hasClass);
			if (hasClass) unitClassText.text = unit.unitClass;

			SetSliderValue(hpSlider, unit.CurrentHp, unit.maxHp);
			SetSliderValue(defenseSlider, unit.CurrentDefense, unit.maxDefense);

			for (int i = apRoot.childCount - 1; i >= 0; i--)
				Destroy(apRoot.GetChild(i).gameObject);
			for (int i = 0; i < unit.CurrentAp; i++)
				Instantiate(apPrefab, apRoot);

			bool hasWeapon = unit.CurrentWeaponContainer != null && unit.CurrentWeaponLogic != null;
			weaponRoot.gameObject.SetActive(hasWeapon);
			if (!hasWeapon) return;
			weaponText.text = unit.CurrentWeaponContainer.Config.nName;
			weaponIcon.sprite = unit.CurrentWeaponContainer.Config.icon;
			weaponIcon.SetNativeSize();
			remainingAmmoText.text = $"{unit.CurrentWeaponLogic.CurrentAmmo()}";
			maxAmmoText.text = $"{unit.CurrentWeaponLogic.AmmoCapacity()}";
		}

        private UnitPortraitView _currentPortrait;

        private void RefreshPortrait(Systems.Unit.Unit unit)
        {
	        if (_currentPortrait) Destroy(_currentPortrait.gameObject);

	        var prefab = unit.portraitPrefabUnitInfo ? unit.portraitPrefabUnitInfo : defaultPortraitPrefab;
	        if (!prefab) return;

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
    }
}
