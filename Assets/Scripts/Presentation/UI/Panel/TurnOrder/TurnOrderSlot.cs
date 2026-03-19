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
		public RectTransform RectTransform => _rect ??= (RectTransform)transform;

		public string UnitId { get; private set; }

		private Tween _moveTween;
		private Tween _scaleTween;
		private Tween _fadeTween;

		public void Setup(string unitId, Sprite icon, Sprite factionBg)
		{
			UnitId = unitId;
			iconImage.sprite = icon;
			iconImage.enabled = icon;
			borderImage.sprite = factionBg;
		}

		private void OnDestroy() => KillAllTweens();

		public void SetState(float scale, float alpha)
		{
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);
			RectTransform.localScale = Vector3.one * scale;
			canvasGroup.alpha = alpha;
		}

		public void AnimateState(float targetScale, float targetAlpha, float duration)
		{
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);
			_scaleTween = RectTransform
				.DOScale(Vector3.one * targetScale, duration)
				.SetEase(stateEase);
			_fadeTween = canvasGroup
				.DOFade(targetAlpha, duration)
				.SetEase(stateEase);
		}

		public void SetX(float targetX)
		{
			KillTween(ref _moveTween);
			var pos = RectTransform.anchoredPosition;
			pos.x = targetX;
			RectTransform.anchoredPosition = pos;
		}

		public void AnimateToX(float targetX, float duration)
		{
			KillTween(ref _moveTween);
			_moveTween = RectTransform
				.DOAnchorPosX(targetX, duration)
				.SetEase(slideEase);
		}

		public void AnimateEntrance(float finalY, float duration, float delay)
		{
			KillTween(ref _moveTween);
			KillTween(ref _fadeTween);

			var pos = RectTransform.anchoredPosition;
			RectTransform.anchoredPosition = new Vector2(pos.x, finalY + entranceOffsetY);
			canvasGroup.alpha = 0f;

			_moveTween = RectTransform
				.DOAnchorPosY(finalY, duration)
				.SetEase(entranceEase)
				.SetDelay(delay);
			_fadeTween = canvasGroup
				.DOFade(1f, duration * 0.6f)
				.SetDelay(delay);
		}

		public void AnimateExit(float duration, Action onComplete = null)
		{
			KillAllTweens();

			DOTween.Sequence()
				.Append(canvasGroup.DOFade(0f, duration).SetEase(exitEase))
				.Join(RectTransform.DOScale(Vector3.zero, duration).SetEase(exitEase))
				.OnComplete(() => onComplete?.Invoke());
		}

		public void ResetForPool()
		{
			KillAllTweens();
			UnitId = null;
			iconImage.sprite = null;
			iconImage.enabled = false;
			RectTransform.localScale = Vector3.one;
			canvasGroup.alpha = 1f;
		}

		private void KillAllTweens()
		{
			KillTween(ref _moveTween);
			KillTween(ref _scaleTween);
			KillTween(ref _fadeTween);
		}

		private void KillTween(ref Tween tween)
		{
			if (tween == null) return;
			tween.Kill();
			tween = null;
		}
	}
}
