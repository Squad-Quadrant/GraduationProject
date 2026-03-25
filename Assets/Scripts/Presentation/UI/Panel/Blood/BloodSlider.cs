using System;
using Presentation.Unit;
using Systems.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class BloodSlider : MonoBehaviour
    {
        private Systems.Unit.Unit _owner;
        private UnitView _unitView;
        [SerializeField] private Image bloodSliderImage;
        [SerializeField] private Image defenseSliderImage;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        private ICoordinateConverter _coordinateConverter;

        // todo: 先搁着刷吧,懒得写事件了,以后再说
        private void FixedUpdate()
        {
            Refresh();
        }

        public void Init(Systems.Unit.Unit owner, ICoordinateConverter coordinateConverter, UnitView unitView)
        {
            _owner = owner;
            _unitView = unitView;
            _coordinateConverter = coordinateConverter;

            Refresh();
        }

        public void OnEnable()
        {
            
        }

        public void OnDisable()
        {
            
        }

        private void Refresh()
        {
            bloodSliderImage.enabled = _unitView.gameObject.activeSelf;
            defenseSliderImage.enabled = _unitView.gameObject.activeSelf;
            bloodSliderImage.fillAmount = Mathf.Lerp(bloodSliderImage.fillAmount, (float)_owner.currentHp / _owner.maxHp, 0.05f);
            defenseSliderImage.fillAmount = Mathf.Lerp(defenseSliderImage.fillAmount, (float)_owner.currentDefense / _owner.defense, 0.05f);
            // Vector3 worldPosition = _coordinateConverter.CellToWorld(_owner.position) + new Vector3(xOffset, yOffset, 0);
            transform.position = Camera.main.WorldToScreenPoint(_unitView.transform.position + new Vector3(xOffset, yOffset, 0));
            
        }
    }
}