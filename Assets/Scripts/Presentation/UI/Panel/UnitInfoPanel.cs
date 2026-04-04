using Core.Events;
using Data.Runtime.Events.Unit;
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
		[SerializeField, Required] private Image portraitImage;
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
            EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitAttacked);
		}
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitAttacked);
        }
        
        private void OnUnitAttacked(UnitInfoChangedEvent e)
        {
            if (e.Unit != _currentUnit) return;
            // DelayUtility.DelayFrame(1, () => );
            Refresh(e.Unit);
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
