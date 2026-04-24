using System.Collections.Generic;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Systems.Damage;
using Systems.Interaction;
using Systems.Unit;
using Systems.Unit.Equipment.Logic;
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
        [SerializeField] private Text damageText;
        [SerializeField] private Text ammoText;
        
        [SerializeField] private Text headRate;        
        [SerializeField] private Text armsRate;        
        [SerializeField] private Text legsRate;        
        [SerializeField] private Text torsoRate;        

        public Dictionary<BodyPartType, Text> bodyPartRate = new();
        
        private IDamageService _damageService;
        private IUnitService _unitService;
        private InteractionContext _interactionContext;
        private Systems.Unit.Unit _unit;
        private WeaponLogic weaponLogic;
        

        public void Init(IDamageService damageService, IUnitService unitService, InteractionContext interactionContext)
        {
            _unitService = unitService;
            _damageService = damageService;
            _interactionContext = interactionContext;
            bodyPartRate.Add(BodyPartType.Head, headRate);
            bodyPartRate.Add(BodyPartType.Arms, armsRate);
            bodyPartRate.Add(BodyPartType.Legs, legsRate);
            bodyPartRate.Add(BodyPartType.Torso, torsoRate);
        }

        protected override void OnOpen()
        {
            EventBus.Subscribe<DisplayHitPercentEvent>(OnDisplayHitPercent);
        }

        protected override void OnClose()
        {
            EventBus.Unsubscribe<DisplayHitPercentEvent>(OnDisplayHitPercent);
        }

        public void DataInitialize(Systems.Unit.Unit unit)
        {
            hitRate.text = "";
            
            _unit = unit;
            weaponLogic = unit.CurrentWeaponLogic;
            if (weaponLogic == null)
            {
                this.Log("当前单位没有持有武器", true);
                return;
            }

            var icon = weaponLogic.Icon();
            equipmentIcon.sprite = icon;
            var fixedRect = new Vector2
            {
                x = icon.rect.width / icon.rect.height * equipmentIcon.rectTransform.sizeDelta.y,
                y = equipmentIcon.rectTransform.sizeDelta.y
            };
            equipmentIcon.rectTransform.sizeDelta = fixedRect;
            equipmentName.text = weaponLogic.Name();
            description.text = weaponLogic.Description();
            
            RefreshInfo(unit.CurrentWeaponLogic.CanPreciseShoot() && unit.CurrentWeaponLogic.IsOnPreciseShoot);

            isPreciseShooting.onValueChanged.RemoveAllListeners();
            isPreciseShooting.gameObject.SetActive(unit.CurrentWeaponLogic.CanPreciseShoot());
            isPreciseShooting.isOn = unit.CurrentWeaponLogic.IsOnPreciseShoot;
            
            if (unit.CurrentWeaponLogic.CanPreciseShoot())
            {
                isPreciseShooting.onValueChanged.AddListener(RefreshInfo);
            }
        }

        private void RefreshInfo(bool isOnPreciseShoot)
        {
            _unit.CurrentWeaponLogic.IsOnPreciseShoot = isOnPreciseShoot;
            int bulletNum = isOnPreciseShoot ? weaponLogic.PreciseShootSpeed() : weaponLogic.ShootSpeed();
            damageText.text = $"0~{bulletNum * weaponLogic.GetDamage()}";
            ammoText.text = bulletNum.ToString();
            
            var bodyRateDic = isOnPreciseShoot ? BodyDestructionConst.PreciseRate : BodyDestructionConst.Rate;
            foreach (var bodyPart in bodyPartRate)
            {
                bodyPart.Value.text = bodyRateDic[bodyPart.Key] * 100 + "%";
            }
        }

        private void OnDisplayHitPercent(DisplayHitPercentEvent e)
        {
            hitRate.text = e.IsValid ? $"命中率: {e.HitPercent}%" : "";
        }
    }
}
