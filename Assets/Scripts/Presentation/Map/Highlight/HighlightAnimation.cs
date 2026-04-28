using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map.Highlight
{
	[RequireComponent(typeof(Tilemap))]
	public class HighlightAnimation : MonoBehaviour
	{
		[SerializeField] private bool on = true;
		[SerializeField] private float alphaSpan = 0.3f;
		[SerializeField] private float duration = 1f;

		private Tilemap _tilemap;

		private void Start()
		{
			if (!on) return;

			_tilemap = GetComponent<Tilemap>();

			var endAlpha = _tilemap.color.a - alphaSpan;

			DOTween.To(
					() => _tilemap.color.a,
					a => { var c = _tilemap.color; c.a = a; _tilemap.color = c; },
					endAlpha,
					duration)
				.SetLoops(-1, LoopType.Yoyo);
		}
	}
}
