using System.Collections.Generic;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map.Highlight
{
	// 单层高亮的封装
	[RequireComponent(typeof(Tilemap))]
	public class HighlightLayer : MonoBehaviour
	{
		[Title("Layer Identity")]
		[SerializeField, LabelText("层 ID"), Tooltip("仅用于日志与调试，不参与业务逻辑")]
		private string layerId = "Unnamed";

		[SerializeField, LabelText("服务的范围类型")]
		[Tooltip("本层响应哪些 ERangeType 的 RangeDisplayEvent")]
		private List<ERangeType> servingRangeTypes = new();

		[Title("Tile")]
		[SerializeField, Required, LabelText("基础 Tile")]
		private TileBase baseTile;

		[Title("Visual")]
		[SerializeField, LabelText("主色")]
		[Tooltip("绘制该层时使用的 tint 颜色")]
		private Color color = new(1f, 1f, 1f, 0.5f);

		// 只有 servingRangeTypes 包含 Movement 时才会用到
		[SerializeField, LabelText("AP 渐变色"), ShowIf("ServingMovement")]
		[Tooltip("仅 Movement 类型使用。索引 0 对应 AP=1，依此类推。超出索引的 AP 使用最后一个颜色。")]
		private Color[] movementApColors = {
			new(0.2f, 0.5f, 1.0f, 0.55f),
			new(0.35f, 0.6f, 1.0f, 0.45f),
			new(0.5f, 0.7f, 1.0f, 0.35f),
			new(0.65f, 0.8f, 1.0f, 0.25f),
		};

		private bool ServingMovement => servingRangeTypes.Contains(ERangeType.Movement);

		private Tilemap _tilemap;

		public string LayerId => layerId;
		public IReadOnlyList<ERangeType> ServingRangeTypes => servingRangeTypes;

		private void Awake() => _tilemap = GetComponent<Tilemap>();

		public void Set(IReadOnlyList<Vector2Int> cells)
		{
			if (!_tilemap) _tilemap = GetComponent<Tilemap>();
			_tilemap.ClearAllTiles();

			if (cells == null || cells.Count == 0) return;

			_tilemap.color = color;
			foreach (var pos in cells)
				_tilemap.SetTile((Vector3Int)pos, baseTile);
		}

		public void SetMovement(IReadOnlyList<Vector2Int> cells, IReadOnlyDictionary<Vector2Int, int> cellCosts)
		{
			if (!_tilemap) _tilemap = GetComponent<Tilemap>();
			_tilemap.ClearAllTiles();

			if (cells == null || cells.Count == 0) return;

			if (movementApColors == null || movementApColors.Length == 0)
			{
				this.LogError($"[{layerId}] Movement 配色为空，回退到主色");
				foreach (var pos in cells)
					SetTileWithColor(pos, color);
				return;
			}

			foreach (var pos in cells)
			{
				int ap = cellCosts != null && cellCosts.TryGetValue(pos, out var cost) ? cost : 1;
				int index = Mathf.Clamp(ap - 1, 0, movementApColors.Length - 1);
				SetTileWithColor(pos, movementApColors[index]);
			}
		}

		public void Clear()
		{
			if (!_tilemap) _tilemap = GetComponent<Tilemap>();
			_tilemap.ClearAllTiles();
		}

		private void SetTileWithColor(Vector2Int pos, Color c)
		{
			var pos3 = (Vector3Int)pos;
			_tilemap.SetTile(pos3, baseTile);
			_tilemap.SetTileFlags(pos3, TileFlags.None);
			_tilemap.SetColor(pos3, c);
		}
	}
}
