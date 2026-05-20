using System;
using Core.Log;
using DG.Tweening;
using UnityEngine;

namespace Systems.Time
{
	public class TimeService : ITimeService, IDisposable
	{
		private Tween _recoverTween;

		public TimeService() => this.Log("Initialized");

		public void SlowMotion(float scale, float durationRealTime, Ease recoverEase = Ease.OutCubic)
		{
			if (scale is <= 0f or >= 1f)
			{
				this.LogWarning($"SlowMotion scale {scale} should be in (0, 1). Clamping.");
				scale = Mathf.Clamp(scale, 0.01f, 0.99f);
			}

			KillRecoverTween();

			UnityEngine.Time.timeScale = scale;

			_recoverTween = DOTween.To(
					() => UnityEngine.Time.timeScale,
					v  => UnityEngine.Time.timeScale = v,
					1f,
					durationRealTime)
				.SetEase(recoverEase)
				.SetUpdate(true)
				.OnComplete(() =>
				{
					UnityEngine.Time.timeScale = 1f;
					_recoverTween = null;
				});

			this.Log($"SlowMotion scale={scale}, durationRealTime={durationRealTime}");
		}

		public void SetTimeScale(float scale)
		{
			KillRecoverTween();
			UnityEngine.Time.timeScale = scale;
		}

		public void ResetTimeScale()
		{
			KillRecoverTween();
			UnityEngine.Time.timeScale = 1f;
		}

		public void Dispose()
		{
			KillRecoverTween();
			UnityEngine.Time.timeScale = 1f;
			this.Log("Disposed");
		}

		private void KillRecoverTween()
		{
			if (_recoverTween == null) return;
			_recoverTween.Kill();
			_recoverTween = null;
		}
	}
}
