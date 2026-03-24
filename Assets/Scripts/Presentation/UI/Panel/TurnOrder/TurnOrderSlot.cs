using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.TurnOrder
{
	public class TurnOrderSlot : MonoBehaviour
	{
		[TitleGroup("References")]
		[SerializeField, Required] private Image iconImage;

		[TitleGroup("References")]
		[SerializeField, Required] private Image borderImage;

		[TitleGroup("References")]
		[SerializeField, Required] private CanvasGroup canvasGroup;

		[TitleGroup("Animation")]
		[SerializeField ]private Ease slideEase = Ease.OutCubic;

		[TitleGroup("Animation")]
		[SerializeField] private Ease stateEase = Ease.OutQuad;

		[TitleGroup("Animation")]
		[SerializeField] private Ease entranceEase = Ease.OutBack;

		[TitleGroup("Animation")]
		[SerializeField] private Ease exitEase = Ease.InQuad;

		[TitleGroup("Animation")]
		[SerializeField] private float entranceOffsetY = 80f;

		private RectTransform _rect;
		public RectTransform Rect => _rect ??= (RectTransform)transform;

		public string UnitId { get; private set; }

		private Tween _slideXTween;
		private Tween _offsetYTween;
		private Tween _scaleTween;
		private Tween _fadeTween;

		private Tween _entranceSeq;
		private Tween _exitSeq;

		public void Setup(string unitId, Sprite icon, Sprite factionBg)
		{
			UnitId = unitId;
			iconImage.sprite = icon;
			iconImage.enabled = icon;
			borderImage.sprite = factionBg;
		}

		public void SetX(float x)
		{
			KillTween(ref _slideXTween);
			var pos = Rect.anchoredPosition;
			pos.x = x;
			Rect.anchoredPosition = pos;
		}

		public void SetVisual(SlotVisual visual)
		{
			KillLifecycleSequences();
			KillTween(ref _offsetYTween);
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);

			var pos = Rect.anchoredPosition;
			pos.y = visual.OffsetY;
			Rect.anchoredPosition = pos;
			Rect.localScale = Vector3.one * visual.Scale;
			canvasGroup.alpha = visual.Alpha;
		}

		public void AnimateToX(float targetX, float duration)
		{
			KillLifecycleSequences();
			KillTween(ref _slideXTween);

			_slideXTween = Rect
				.DOAnchorPosX(targetX, duration)
				.SetEase(slideEase);
		}

		public void AnimateVisual(SlotVisual visual, float duration)
		{
			KillLifecycleSequences();
			KillTween(ref _offsetYTween);
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);

			_offsetYTween = Rect
				.DOAnchorPosY(visual.OffsetY, duration)
				.SetEase(stateEase);

			_scaleTween = Rect
				.DOScale(Vector3.one * visual.Scale, duration)
				.SetEase(stateEase);

			_fadeTween = canvasGroup
				.DOFade(visual.Alpha, duration)
				.SetEase(stateEase);
		}

		public void PlayEntrance(SlotVisual targetVisual, float duration, float delay)
		{
			KillAllTweens();

			// Start state: above target position, invisible, at target scale
			var pos = Rect.anchoredPosition;
			Rect.anchoredPosition = new Vector2(pos.x, targetVisual.OffsetY + entranceOffsetY);
			Rect.localScale = Vector3.one * targetVisual.Scale;
			canvasGroup.alpha = 0f;

			const float fadeRatio = 0.6f;

			_entranceSeq = DOTween.Sequence()
				.SetDelay(delay)
				.Append(
					Rect.DOAnchorPosY(targetVisual.OffsetY, duration)
						.SetEase(entranceEase))
				.Join(
					canvasGroup.DOFade(targetVisual.Alpha, duration * fadeRatio))
				// Clear reference on natural completion so KillAllTweens won't
				// try to kill an already-dead sequence
				.OnKill(() => _entranceSeq = null);
		}

		public void PlayExit(float duration, Action onComplete = null)
		{
			KillAllTweens();

			_exitSeq = DOTween.Sequence()
				.Append(
					canvasGroup.DOFade(0f, duration)
						.SetEase(exitEase))
				.Join(
					Rect.DOScale(Vector3.zero, duration)
						.SetEase(exitEase))
				.OnKill(() => _exitSeq = null)
				.OnComplete(() => onComplete?.Invoke());
		}

		public void ResetForPool()
		{
			KillAllTweens();

			UnitId = null;
			iconImage.sprite = null;
			iconImage.enabled = false;
			Rect.localScale = Vector3.one;
			Rect.anchoredPosition = Vector2.zero;
			canvasGroup.alpha = 1f;
		}

		private void OnDestroy() => KillAllTweens();

		private void KillAllTweens()
		{
			KillTween(ref _slideXTween);
			KillTween(ref _offsetYTween);
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);
			KillTween(ref _entranceSeq);
			KillTween(ref _exitSeq);
		}

		private void KillLifecycleSequences()
		{
			KillTween(ref _entranceSeq);
			KillTween(ref _exitSeq);
		}

		private static void KillTween(ref Tween tween)
		{
			if (tween == null) return;
			if (tween.active) tween.Kill();
			tween = null;
		}
	}
}
