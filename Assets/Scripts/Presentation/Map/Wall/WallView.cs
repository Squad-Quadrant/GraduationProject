using Sirenix.OdinInspector;
using Systems.Map;
using UnityEngine;

namespace Presentation.Map.Wall
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class WallView : MonoBehaviour
	{
		[SerializeField, ReadOnly] private Vector2Int cellA;
		[SerializeField, ReadOnly] private Vector2Int cellB;

		private SpriteRenderer _spriteRenderer;

		public WallKey WallKey => new(cellA, cellB);

		public SpriteRenderer Renderer => _spriteRenderer
			? _spriteRenderer
			: _spriteRenderer = GetComponent<SpriteRenderer>();

		public void Setup(Vector2Int a, Vector2Int b)
		{
			cellA = a;
			cellB = b;
			Renderer.sortingLayerName = "OnGround";
		}
	}
}
