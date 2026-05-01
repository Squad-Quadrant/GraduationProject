using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.AreaEffect
{
	[DisallowMultipleComponent]
	public class AreaEffectVfxBehavior : MonoBehaviour
	{
		[Title("Fade Out")]
		[LabelText("Fade-out 时长 (秒)"), MinValue(0f)]
		[SerializeField] private float fadeOutDuration = 1f;

		[LabelText("停止发射的粒子系统")]
		[SerializeField] private List<ParticleSystem> particleSystems = new();

		[LabelText("Alpha 渐隐的 SpriteRenderer")]
		[SerializeField] private List<SpriteRenderer> fadingSprites = new();

		private bool _fading;

		public void FadeOutAndDestroy()
		{
			if (_fading) return;
			_fading = true;

			foreach (var ps in particleSystems.Where(ps => ps))
				ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			foreach (var sr in fadingSprites.Where(sr => sr))
				sr.DOFade(0f, fadeOutDuration).SetUpdate(true);

			Destroy(gameObject, fadeOutDuration);
		}

		private void OnDestroy()
		{
			foreach (var sr in fadingSprites.Where(sr => sr))
				sr.DOKill();
		}
	}
}
