using System;
using PurpleFlowerCore.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.Component
{
    enum BarType
    {
        LeftToRight,
        RightToLeft,
    }

    enum RenderType
    {
        ScreenSpace_Camera,
        ScreenSpace_Overlay,
    }
    
    enum UpdateType
    {
        ByCall,
        ByFrame

    }
    [AddComponentMenu("PFC/PropertyBar")]
    public class PropertyBar : MonoBehaviour
    {
        // [SerializeField] private Transform leftPoint;
        // [SerializeField] private Transform rightPoint;
        [SerializeField] private BarType barType;
        [SerializeField] private RenderType renderType;
        [SerializeField] private UpdateType updateType;
        [SerializeField] private Mask barMask;
        [SerializeField] private Image barImage;
        private Vector3 _edgePosition;
        public Vector3 EdgePosition => _edgePosition;
        private Vector3 _initImagePosition;
        public Vector3 InitImagePosition => _initImagePosition;
        [RO][SerializeField]private float value;
        [SerializeField] private Vector3 imageOffset;
        private Vector3 _initPosition;

        public float Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        private void Awake()
        {
            _initImagePosition = barImage.transform.position;
            _initPosition = transform.position;
        }

        private void Update()
        {
            if(updateType == UpdateType.ByFrame)
                SetValue(value);
        }

        public void SetValue(float value)
        {
            Vector3 offset1;
            Vector3 offset2 = transform.position - _initPosition;
            if (renderType == RenderType.ScreenSpace_Camera)
            {
                offset1 = barImage.rectTransform.rect.width / 2 * barImage.rectTransform.lossyScale.x * barImage.rectTransform.right ;
            }
            else
            {
                offset1 = barImage.rectTransform.rect.width / 2 * barImage.rectTransform.right;
            }

            this.value = Mathf.Clamp01(value);
            var leftPosition = barImage.rectTransform.position - offset1;
            var rightPosition = barImage.rectTransform.position + offset1;
            var center = (leftPosition + rightPosition) / 2;
            if (barType == BarType.RightToLeft)
            {
                barMask.rectTransform.position =
                    Vector3.Lerp(center, leftPosition + (leftPosition - rightPosition) / 2, this.value);
                barImage.transform.position = _initImagePosition + imageOffset + offset2;
                _edgePosition = Vector3.Lerp(rightPosition, leftPosition, this.value);
            }
            else
            {
                barMask.rectTransform.position = Vector3.Lerp(center, rightPosition + (rightPosition - leftPosition) / 2, this.value);
                barImage.transform.position = _initImagePosition + imageOffset + offset2;
                _edgePosition = Vector3.Lerp(leftPosition, rightPosition, this.value);
            }
        }

        public float GetValue()
        {
            return value;
        }

    }
}