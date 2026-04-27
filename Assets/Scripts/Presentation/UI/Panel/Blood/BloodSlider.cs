using Core.Events;
using Data.Runtime.Events.Damage;
using DG.Tweening;
using Presentation.UI.Component.Buff;
using Presentation.Unit;
using Systems.Interfaces;
using Systems.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Blood
{
    public class BloodSlider : MonoBehaviour
    {
        private Systems.Unit.Unit _owner;
        private UnitView _unitView;
        
        [SerializeField] private Text nameText;
        
        [SerializeField] private Image bloodSliderImage;
        [SerializeField] private Image bloodSliderImage1;
        [SerializeField] private Image bloodSliderImage2;
        [SerializeField] private Image defenseSliderImage;
        [SerializeField] private Image defenseSliderImage1;
        [SerializeField] private Image defenseSliderImage2;

        [SerializeField] private Image playerBackground;
        [SerializeField] private Image enemyBackground;
        
        [SerializeField] private BuffList buffList;
        
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float minSpeed = 0.01f;
        [SerializeField] private float speed1 = 0.05f;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        
        [SerializeField] private CanvasGroup canvasGroup;
        private ICoordinateConverter _coordinateConverter;
        private IEventBus _eventBus;
        private float _originCameraSize;

        private float hpTarget = 1;
        private float hpTarget1 = 1;
        private float defenseTarget = 1;
        private float defenseTarget1 = 1;

        private float hpSpeed;
        private float defenseSpeed;

        private void FixedUpdate()
        {
            Refresh();
        }

        public void Init(Systems.Unit.Unit owner, ICoordinateConverter coordinateConverter, UnitView unitView, IEventBus eventBus)
        {
            _originCameraSize = Camera.main.orthographicSize;
            _owner = owner;
            _unitView = unitView;
            _coordinateConverter = coordinateConverter;
            _eventBus = eventBus;
            
            nameText.text = _owner.name;
            if (owner.faction == EUnitFaction.Player)
            {
                enemyBackground.enabled = false;
            }
            else
            {
                playerBackground.enabled = false;
            }
            
            buffList.Init(owner);
            
            Refresh();
            
            _eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void Refresh()
        {
            canvasGroup.alpha = _unitView.GetVisible() ? 1 : 0;
            
            bool defenseSliderOn = _owner.maxDefense > 0;
            defenseSliderImage.enabled = defenseSliderOn;
            defenseSliderImage1.enabled = defenseSliderOn;
            defenseSliderImage2.enabled = defenseSliderOn;
            
            bloodSliderImage.fillAmount = Mathf.MoveTowards(bloodSliderImage.fillAmount, hpTarget, hpSpeed * Time.fixedDeltaTime);
            bloodSliderImage1.fillAmount = Mathf.Lerp(bloodSliderImage1.fillAmount, hpTarget1, speed1);
            defenseSliderImage.fillAmount = Mathf.MoveTowards(defenseSliderImage.fillAmount, defenseTarget, defenseSpeed * Time.fixedDeltaTime);
            defenseSliderImage1.fillAmount = Mathf.Lerp(defenseSliderImage1.fillAmount, defenseTarget1, speed1);
            transform.position = _unitView.transform.position + new Vector3(xOffset, yOffset, 0);
            transform.localScale =  _originCameraSize * Vector3.one / Camera.main.orthographicSize;
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            if (e.Context.Defender != _owner) return;
            hpTarget1 = (float)_owner.CurrentHp / _owner.maxHp;
            defenseTarget1 = (float)_owner.CurrentDefense / _owner.maxDefense;
            DOVirtual.DelayedCall(2f, () =>
            {
	            hpTarget = hpTarget1;
	            hpSpeed = Mathf.Abs(hpTarget - bloodSliderImage.fillAmount) / duration;
	            hpSpeed = Mathf.Max(hpSpeed, minSpeed);
	            defenseTarget = defenseTarget1;
	            defenseSpeed = Mathf.Abs(defenseTarget - defenseSliderImage.fillAmount) / duration;
	            defenseSpeed = Mathf.Max(defenseSpeed, minSpeed);
            });
        }
    }
}
