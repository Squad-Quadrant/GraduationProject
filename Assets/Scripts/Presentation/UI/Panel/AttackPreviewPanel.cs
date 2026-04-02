using Core.Log;
using Presentation.UI.Core;
using Systems.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class AttackPreviewPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
        [SerializeField] private Image equipmentIcon;
        [SerializeField] private Text equipmentName;
        [SerializeField] private Text description;
        [SerializeField] private Text hitChance;
        [SerializeField] private Toggle isPreciseShooting;
        public void DataInitialize(Systems.Unit.Unit unit)
        {
            var currentEquipment = unit.CurrentEquipment;
            if (currentEquipment.IsNullOrEmpty())
            {
                this.LogError("当前武器为空");
                return;
            }

            var config = currentEquipment.Config; 
            equipmentIcon.sprite = config.Icon;
            var fixedRect = new Vector2()
            {
                x = config.Icon.rect.width / config.Icon.rect.height * equipmentIcon.rectTransform.sizeDelta.y,
                y = equipmentIcon.rectTransform.sizeDelta.y
            };
            equipmentIcon.rectTransform.sizeDelta = fixedRect;
            equipmentName.text = config.Name;
            description.text = config.Description;

            isPreciseShooting.onValueChanged.RemoveAllListeners();
            isPreciseShooting.gameObject.SetActive(currentEquipment.Config.canPreciseShoot);
            isPreciseShooting.isOn = unit.CurrentWeapon.isOnPreciseShoot;

            if (unit.CurrentWeapon.CanPreciseShoot())
            {
                isPreciseShooting.onValueChanged.AddListener(isOn =>
                {
                    unit.CurrentWeapon.isOnPreciseShoot = isOn;
                });
            }
        }
    }
}