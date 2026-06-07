using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using UnityEngine;

namespace Presentation.Map.SceneActor
{
	public class SceneActorView : MonoBehaviour
	{
		[SerializeField, ReadOnly] private uint uid;

		private readonly List<SpriteRenderer> _sliceRenderers = new();

		public uint Uid => uid;

		public void Setup(uint id, Vector2Int basePosition, IReadOnlyList<SpriteSlice> slices, ICoordinateConverter converter)
		{
			uid = id;
			BuildSlices(basePosition, slices, converter);
		}

		public void RefreshSlices(Vector2Int basePosition, IReadOnlyList<SpriteSlice> slices, ICoordinateConverter converter)
		{
			ClearSlices();
			BuildSlices(basePosition, slices, converter);
		}

		public void SetAlpha(float alpha)
		{
			foreach (var r in _sliceRenderers)
			{
				var c = r.color;
				c.a = alpha;
				r.color = c;
			}
		}

		public void SetHighlight(bool highlighted)
		{
			foreach (var r in _sliceRenderers)
			{
				var alpha = r.color.a;
				var color = highlighted ? Color.red : Color.white;
				color.a = alpha;
				r.color = color;
			}
		}

		private void BuildSlices(Vector2Int basePosition, IReadOnlyList<SpriteSlice> slices, ICoordinateConverter converter)
		{
			if (slices == null) return;

			foreach (var slice in slices)
			{
				if (slice == null || !slice.sprite) continue;

				var cellPos = basePosition + slice.cellOffset;
				var cellCenter = converter.CellToWorld(cellPos);

				var go = new GameObject($"Slice_{slice.cellOffset.x}_{slice.cellOffset.y}");
				go.transform.SetParent(transform, worldPositionStays: false);
				go.transform.position = cellCenter;

				var sR = go.AddComponent<SpriteRenderer>();
				sR.sprite = slice.sprite;
				sR.sortingLayerName = "OnGround";
				sR.spriteSortPoint = SpriteSortPoint.Pivot; // Y-sort 用 pivot 而非 GameObject center

				_sliceRenderers.Add(sR);
			}
		}

		private void ClearSlices()
		{
			foreach (var r in _sliceRenderers.Where(r => r))
				Destroy(r.gameObject);
			_sliceRenderers.Clear();
		}
	}
}
