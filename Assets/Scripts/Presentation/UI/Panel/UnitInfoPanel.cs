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
		[SerializeField, Required] private Transform portraitContainer;
		[SerializeField, Required] private UnitPortraitView defaultPortraitPrefab;
		[SerializeField, Required] private TextMeshProUGUI nameText;
		[SerializeField, Required] private TextMeshProUGUI bulletAmountText;
		[SerializeField, Required] private Image hpImage;
		[SerializeField, Required] private Image defenseImage;
		[SerializeField, Required] private RectTransform actionPointsParent;
		[SerializeField, Required] private GameObject actionPointsPrefab;
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
			nameText.text = unit.name;

			var currentHp = unit.CurrentHp;
			var maxHp = unit.maxHp;
			hpImage.fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            defenseImage.enabled = unit.defense > 0;
            defenseImage.fillAmount = (float)unit.CurrentDefense / unit.defense;

			foreach (Transform child in actionPointsParent)
				Destroy(child.gameObject);
			for (int i = 0; i < unit.CurrentAp; i++)
				Instantiate(actionPointsPrefab, actionPointsParent);

            RefreshAmmo(unit);
        }

        private UnitPortraitView _currentPortrait;

        private void RefreshPortrait(Systems.Unit.Unit unit)
        {
	        if (_currentPortrait) Destroy(_currentPortrait.gameObject);

	        var prefab = unit.portraitPrefab ? unit.portraitPrefab : defaultPortraitPrefab;
	        if (!prefab) return;

	        _currentPortrait = Instantiate(prefab, portraitContainer);
	        _currentPortrait.Init();
        }

        private void RefreshAmmo(Systems.Unit.Unit unit)
        {
            var currentWeapon = unit.CurrentWeapon;
            if (currentWeapon != null)
            {
                bulletAmountText.enabled = true;
                bulletAmountText.text =
                    $"{currentWeapon.CurrentAmmo()}/{currentWeapon.AmmoCapacity()}";
            }
            else
            {
                bulletAmountText.enabled = false;
            }
        }
    }
}
