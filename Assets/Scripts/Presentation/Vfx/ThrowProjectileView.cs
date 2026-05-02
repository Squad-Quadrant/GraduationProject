using System;
using Core.Log;
using DG.Tweening;
using UnityEngine;

namespace Presentation.Vfx
{
	public class ThrowProjectileView : MonoBehaviour
	{
		[Tooltip("Z 轴旋转")]
		[SerializeField] private bool spinDuringFlight = true;

		[Tooltip("每秒旋转度数")]
		[SerializeField] private float spinSpeedDegPerSec = 720f;

		private Tween _flightTween;

		public void Launch(Vector3 from, Vector3 to, float duration, float arcHeight, Action onLanded)
		{
			transform.position = from;

			float t = 0f;
			_flightTween = DOTween.To(
					() => t,
					v =>
					{
						t = v;
						var basePos = Vector3.Lerp(from, to, t);
						var arc = Vector3.up * (arcHeight * 4f * t * (1f - t));
						transform.position = basePos + arc;
					}, 1f, duration)
				.SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					transform.position = to;
					_flightTween = null;
					onLanded?.Invoke();
				});

			if (spinDuringFlight)
			{
				transform
					.DORotate(new Vector3(0, 0, spinSpeedDegPerSec * duration), duration, RotateMode.FastBeyond360)
					.SetEase(Ease.Linear);
			}

			this.Log($"Launched: {from} → {to}, duration={duration:F2}s, arc={arcHeight:F2}");
		}

		private void OnDestroy()
		{
			_flightTween?.Kill(false);
			_flightTween = null;
			DOTween.Kill(transform);
		}
	}
}
