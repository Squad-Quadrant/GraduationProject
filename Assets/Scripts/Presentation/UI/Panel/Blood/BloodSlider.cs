using System;
using Systems.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class BloodSlider : MonoBehaviour
    {
        private Systems.Unit.Unit _owner;
        [SerializeField] private Image bloodSliderImage;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        private ICoordinateConverter _coordinateConverter;

        // todo: 先搁着刷吧,懒得写事件了,以后再说
        private void Update()
        {
            Refresh();
        }

        public void Init(Systems.Unit.Unit owner, ICoordinateConverter coordinateConverter)
        {
            _owner = owner;
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
            bloodSliderImage.fillAmount = (float)_owner.currentHp / _owner.maxHp;
            Vector3 worldPosition = _coordinateConverter.CellToWorld(_owner.position) + new Vector3(xOffset, yOffset, 0);
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }
    }
}