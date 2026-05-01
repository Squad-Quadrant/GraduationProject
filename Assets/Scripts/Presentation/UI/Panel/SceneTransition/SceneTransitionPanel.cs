using System;
using DG.Tweening;
using Presentation.UI.Core;
using Sirenix.OdinInspector;

namespace Presentation.UI.Panel.SceneTransition
{
	public class SceneTransitionPanel : UIPanel
	{
		private Tween _activeTween;

		[ShowInInspector, ReadOnly]
		private float CurrentAlpha => CanvasGroup ? CanvasGroup.alpha : -1f;

		protected override void OnOpen()
		{
			KillTween();
			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = false;
			CanvasGroup.interactable = false;
		}

		protected override void OnClose() => KillTween();

		protected override void OnDestroy()
		{
			KillTween();
			base.OnDestroy();
		}

		public void FadeToBlack(float duration, Action onComplete)
		{
			KillTween();

			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = true;

			_activeTween = CanvasGroup
				.DOFade(1f, duration)
				.SetEase(Ease.InOutQuad)
				.SetUpdate(true)
				.OnComplete(() =>
				{
					_activeTween = null;
					onComplete?.Invoke();
				});
		}

		public void FadeFromBlack(float duration, Action onComplete)
		{
			KillTween();

			CanvasGroup.alpha = 1f;

			_activeTween = CanvasGroup
				.DOFade(0f, duration)
				.SetEase(Ease.InOutQuad)
				.SetUpdate(true)
				.OnComplete(() =>
				{
					_activeTween = null;
					CanvasGroup.blocksRaycasts = false;
					onComplete?.Invoke();
				});
		}

		private void KillTween()
		{
			if (_activeTween == null) return;
			_activeTween.Kill(complete: false);
			_activeTween = null;
		}
	}
}
