using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Component
{
	[RequireComponent(typeof(Button))]
	public class UIButtonHoverScale : MonoBehaviour,
		IPointerEnterHandler,
		IPointerExitHandler,
		IPointerUpHandler,
		IPointerDownHandler
	{
		[FoldoutGroup("To Max (Hover)")] [Range(1f, 1.3f)]
		[SerializeField] private float maxScale = 1.1f;

		[FoldoutGroup("To Max (Hover)")] [Range(0.02f, 0.5f)] [SerializeField]
		private float toMaxDuration = 0.15f;

		[FoldoutGroup("To Max (Hover)")] [SerializeField]
		private Ease toMaxEase = Ease.OutBack;

		[FoldoutGroup("To Min (Press)")] [Range(0.5f, 1f)] [SerializeField]
		private float minScale = 0.85f;

		[FoldoutGroup("To Min (Press)")] [Range(0.01f, 0.15f)] [SerializeField]
		private float toMinDuration = 0.06f;

		[FoldoutGroup("To Min (Press)")] [SerializeField]
		private Ease toMinEase = Ease.OutQuad;

		[FoldoutGroup("To Origin (Idle)")] [Range(0.02f, 0.5f)] [SerializeField]
		private float toOriginDuration = 0.12f;

		[FoldoutGroup("To Origin (Idle)")] [SerializeField]
		private Ease toOriginEase = Ease.OutQuad;

		// --- Runtime ---
		private Vector3 _originalScale;
		private Tween _currentTween;
		private Button _button;
		private bool _isHovered;
		private bool _isPressed;

		private void Awake()
		{
			_originalScale = transform.localScale;
			_button = GetComponent<Button>();
		}

		// --- Pointer events: set/clear flags, then let ApplyState decide ---

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isHovered = true;
			ApplyState();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isHovered = false;
			ApplyState();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_isPressed = true;
			ApplyState();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_isPressed = false;
			ApplyState();
		}

		// --- Core: pick a target based on current flags ---

		private void ApplyState()
		{
			if (!_button.interactable)
			{
				KillTween();
				transform.localScale = _originalScale;
				return;
			}

			// Priority: Pressed > Hovered > Idle
			if (_isPressed)
				AnimateTo(_originalScale * minScale, toMinDuration, toMinEase);
			else if (_isHovered)
				AnimateTo(_originalScale * maxScale, toMaxDuration, toMaxEase);
			else
				AnimateTo(_originalScale, toOriginDuration, toOriginEase);
		}

		private void AnimateTo(Vector3 target, float duration, Ease ease)
		{
			KillTween();

			_currentTween = transform
				.DOScale(target, duration)
				.SetEase(ease)
				.SetAutoKill(false)
				.SetUpdate(true)
				.SetLink(gameObject);
		}

		private void KillTween()
		{
			if (_currentTween != null && _currentTween.IsActive())
				_currentTween.Kill();

			_currentTween = null;
		}

		private void OnDisable()
		{
			KillTween();
			transform.localScale = _originalScale;
			_isHovered = false;
			_isPressed = false;
		}
	}
}
