using System;
using DG.Tweening;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Component
{
	public class UIPresetAnimation : MonoBehaviour, IUIAnimation
	{
		private enum ESlidePivot { Top, Bottom, Left, Right }

		[TitleGroup("Target")]
		[SerializeField] private RectTransform target;

		[TitleGroup("Slide")]
		[SerializeField, ToggleLeft] private bool useSlide;

		[TitleGroup("Slide"), ShowIf(nameof(useSlide))]
		[SerializeField, Tooltip("起始方向。Bottom 即从下方滑入，关闭时滑回下方。")]
		private ESlidePivot slidePivot = ESlidePivot.Bottom;

		[TitleGroup("Slide"), ShowIf(nameof(useSlide))]
		[SerializeField] private float slideOffset = 80f;

		[TitleGroup("Fade")]
		[SerializeField, ToggleLeft] private bool useFade = true;

		[TitleGroup("Scale")]
		[SerializeField, ToggleLeft] private bool useScale;

		[TitleGroup("Scale"), ShowIf(nameof(useScale))]
		[SerializeField, MinValue(0f), Tooltip("起始/结束缩放系数（相对于初始 scale）。0 = 完全缩起，1 = 不缩。")]
		private float scaleFrom = 0.9f;

		[TitleGroup("Open")]
		[SerializeField, Range(0.05f, 1f)] private float openDuration = 0.3f;

		[TitleGroup("Open")]
		[SerializeField] private Ease openEase = Ease.OutCubic;

		[TitleGroup("Close")]
		[SerializeField, Range(0.05f, 1f)] private float closeDuration = 0.2f;

		[TitleGroup("Close")]
		[SerializeField] private Ease closeEase = Ease.InCubic;

		private CanvasGroup _canvasGroup;
		private Vector2 _restPosition;
		private Vector3 _restScale;
		private Sequence _sequence;

		public bool IsAnimating => _sequence is { active: true };

		private void Awake()
		{
			if (!target) target = (RectTransform)transform;
			_canvasGroup = GetComponent<CanvasGroup>();

			_restPosition = target.anchoredPosition;
			_restScale    = target.localScale;
		}

		public void PlayOpen(Action onComplete)
		{
			KillSequence();
			ApplyOpenStartState();
			_sequence = BuildOpenSequence(onComplete);
		}

		public void PlayClose(Action onComplete)
		{
			KillSequence();
			_sequence = BuildCloseSequence(onComplete);
		}

		public void CompleteImmediately()
		{
			KillSequence();
			target.anchoredPosition = _restPosition;
			target.localScale       = _restScale;
		}

		private void OnDestroy() => KillSequence();

		private void ApplyOpenStartState()
		{
			if (useSlide) target.anchoredPosition = _restPosition + GetSlideOffset();
			if (useFade)  _canvasGroup.alpha      = 0f;
			if (useScale) target.localScale       = _restScale * scaleFrom;
		}

		private Sequence BuildOpenSequence(Action onComplete)
		{
			var seq = DOTween.Sequence();
			if (useSlide) seq.Join(target.DOAnchorPos(_restPosition, openDuration).SetEase(openEase));
			if (useFade)  seq.Join(_canvasGroup.DOFade(1f, openDuration).SetEase(openEase));
			if (useScale) seq.Join(target.DOScale(_restScale, openDuration).SetEase(openEase));
			return ConfigureSequence(seq, onComplete);
		}

		private Sequence BuildCloseSequence(Action onComplete)
		{
			var seq = DOTween.Sequence();
			if (useSlide) seq.Join(target.DOAnchorPos(_restPosition + GetSlideOffset(), closeDuration).SetEase(closeEase));
			if (useFade)  seq.Join(_canvasGroup.DOFade(0f, closeDuration).SetEase(closeEase));
			if (useScale) seq.Join(target.DOScale(_restScale * scaleFrom, closeDuration).SetEase(closeEase));
			return ConfigureSequence(seq, onComplete);
		}

		private Sequence ConfigureSequence(Sequence seq, Action onComplete)
		{
			seq.SetUpdate(true);

			seq.OnComplete(() =>
			{
				_sequence = null;
				onComplete?.Invoke();
			});

			seq.OnKill(() =>
			{
				if (_sequence == seq) _sequence = null;
			});

			return seq;
		}

		private void KillSequence()
		{
			if (_sequence == null) return;
			_sequence.Kill(complete: false);
			_sequence = null;
		}

		private Vector2 GetSlideOffset() => slidePivot switch
		{
			ESlidePivot.Top    => Vector2.up    * slideOffset,
			ESlidePivot.Bottom => Vector2.down  * slideOffset,
			ESlidePivot.Left   => Vector2.left  * slideOffset,
			ESlidePivot.Right  => Vector2.right * slideOffset,
			_ => Vector2.zero
		};
	}
}
