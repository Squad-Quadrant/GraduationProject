using System;
using Core.Events;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using PurpleFlowerCore.Utility;
using Sirenix.OdinInspector;
using Systems.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public class UnitInfoPanel : UIPanel, IInitializable<Systems.Unit.Unit>, IDisposable
	{
		[TitleGroup("References")]
		[SerializeField, Required] private Image portraitImage;
		[SerializeField, Required] private TextMeshProUGUI nameText;
		[SerializeField, Required] private TextMeshProUGUI bulletAmountText;
		[SerializeField, Required] private Image hpImage;
		[SerializeField, Required] private Image defenseImage;
		[SerializeField, Required] private RectTransform actionPointsParent;
		[SerializeField, Required] private GameObject actionPointsPrefab;
        private IEventBus _eventBus;
        private Systems.Unit.Unit _currentUnit;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			if (unit == null) return;
            _currentUnit = unit;
			Refresh(unit);
		}
        
        public void Init(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<UnitAttackedEvent>(OnUnitAttacked);
        }
        
        public void Dispose()
        {
            _eventBus.Unsubscribe<UnitAttackedEvent>(OnUnitAttacked);
        }

        public void Refresh(Systems.Unit.Unit unit)
		{
			portraitImage.sprite = unit.icon;
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

        private void OnUnitAttacked(UnitAttackedEvent e)
        {
            if (e.Attacker != _currentUnit) return;
            DelayUtility.DelayFrame(1, () => RefreshAmmo(e.Attacker));
            
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
