using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Input;
using Presentation.UI.Core;
using Systems.Damage;
using Systems.Equipment;
using Systems.Interaction;
using Systems.Unit;
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

        protected override void OnInitialize()
        {
            base.OnInitialize();
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
        }
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
            base.OnDestroy();
        }

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
            equipmentIcon.sprite = config.Icon;
            var fixedRect = new Vector2
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



        private void OnPointerHover(PointerHoverEvent e)
        {
            if (e.HoveredUnitId != null && _unitService.HasUnit(e.HoveredUnitId) && _interactionContext.validTargetCells.Contains(e.CellPosition.Value))
            {
                var target = _unitService.GetUnit(e.HoveredUnitId);
                var damageContext = _damageService.GetSimulatedDamage(new DamageTriggeringInfo(DamageType.Bullet, _unit, target, EActionType.Attack));
                hitRate.text = "命中率: " + Mathf.RoundToInt(damageContext.HitRate * 100) + "%";
            }
            else
            {
                hitRate.text = "";
                
            }
        }
    }
}