using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Systems.Damage;
using Systems.Interaction;
using Systems.Unit;
using Systems.Unit.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class AttackPreviewPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
        [SerializeField] private Image equipmentIcon;
        [SerializeField] private Text equipmentName;
        [SerializeField] private Text description;
        [SerializeField] private Text hitRate;
        [SerializeField] private Toggle isPreciseShooting;
        private IDamageService _damageService;
        private IUnitService _unitService;
        private InteractionContext _interactionContext;
        private Systems.Unit.Unit _unit;
        

        public void Init(IDamageService damageService, IUnitService unitService, InteractionContext interactionContext)
        {
            _unitService = unitService;
            _damageService = damageService;
            _interactionContext = interactionContext;
        }

        protected override void OnOpen() => EventBus.Subscribe<DisplayHitPercentEvent>(OnDisplayHitPercent);

        protected override void OnClose() => EventBus.Unsubscribe<DisplayHitPercentEvent>(OnDisplayHitPercent);

        public void DataInitialize(Systems.Unit.Unit unit)
        {
            var currentEquipment = unit.CurrentEquipment;
            if (currentEquipment.IsNullOrEmpty())
            {
                this.LogError("当前武器为空");
                return;
            }

            hitRate.text = "";

            _unit = unit;
            var config = currentEquipment.Config; 
            equipmentIcon.sprite = config.icon;
            var fixedRect = new Vector2
            {
                x = config.icon.rect.width / config.icon.rect.height * equipmentIcon.rectTransform.sizeDelta.y,
                y = equipmentIcon.rectTransform.sizeDelta.y
            };
            equipmentIcon.rectTransform.sizeDelta = fixedRect;
            equipmentName.text = config.nName;
            description.text = config.description;

            isPreciseShooting.onValueChanged.RemoveAllListeners();
            isPreciseShooting.gameObject.SetActive(unit.CurrentWeapon.CanPreciseShoot());
            isPreciseShooting.isOn = unit.CurrentWeapon.IsOnPreciseShoot;

            if (unit.CurrentWeapon.CanPreciseShoot())
            {
                isPreciseShooting.onValueChanged.AddListener(isOn =>
                {
                    unit.CurrentWeapon.IsOnPreciseShoot = isOn;
                });
            }
        }

        private void OnDisplayHitPercent(DisplayHitPercentEvent e) => hitRate.text = e.IsValid ? $"命中率: {e.HitPercent}%" : "";
    }
}
