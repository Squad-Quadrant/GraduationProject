using System.Collections;
using Systems.Buff;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Component.Buff
{
    public class BuffListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private float hoverDelay = 2f;

        private BuffInfo _info;
        private BuffTooltip _tooltip;
        private Coroutine _hoverRoutine;

        public void Init(BuffInfo info, BuffTooltip tooltip)
        {
	        _info = info;
	        _tooltip = tooltip;

            icon.sprite = info.BuffData.icon;
            aspectRatioFitter.aspectRatio = info.BuffData.icon.rect.width / info.BuffData.icon.rect.height;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
	        if (!_tooltip) return;
	        _hoverRoutine = StartCoroutine(HoverCountdown());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
	        CancelHover();
	        if (_tooltip) _tooltip.Hide();
        }

        private IEnumerator HoverCountdown()
        {
	        yield return new WaitForSeconds(hoverDelay);
	        _hoverRoutine = null;
	        _tooltip.Show(_info, (RectTransform)transform);
        }

        private void CancelHover()
        {
	        if (_hoverRoutine == null) return;
	        StopCoroutine(_hoverRoutine);
	        _hoverRoutine = null;
        }

        private void OnDisable()
        {
	        CancelHover();
	        if (_tooltip) _tooltip.Hide();
        }
    }
}
