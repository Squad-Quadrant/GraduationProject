using System.Collections.Generic;
using Core.Events;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Data.Runtime.Events.View;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
    /// <summary>
    /// MapView 目前，该组件承担渲染所有与地图相关的视觉元素的责任，包括地形、墙壁、单位和高亮显示等，而不只是地图。
    /// </summary>
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private Tilemap groundTilemap;
        [SerializeField, Required] private Tilemap leftWallTilemap;
        [SerializeField, Required] private Tilemap rightTilemap;
        [SerializeField, Required] private Tilemap sceneActorTilemap;
        [SerializeField, Required] private Tilemap highlightTilemap;
        [SerializeField, Required] private Tilemap pathTilemap;

        [Title("Highlight")]
        [SerializeField, Required] private RuleTile highlightRuleTile;
        [SerializeField] private Color selectionHighlightColor = new(1f, 1f, 0.6f, 0.65f);
        [SerializeField] private Color[] moveApColors = {
	        new(0.2f, 0.5f, 1.0f, 0.55f),  // AP 1 — deepest blue
	        new(0.35f, 0.6f, 1.0f, 0.45f), // AP 2
	        new(0.5f, 0.7f, 1.0f, 0.35f),  // AP 3
	        new(0.65f, 0.8f, 1.0f, 0.25f), // AP 4 — lightest
        };
        [SerializeField] private Color attackRangeColor = new(1f, 0.3f, 0.3f, 0.5f);

        [Title("Path Preview")]
        [SerializeField] private TileBase[] pathTiles = new TileBase[10];
        
        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

        private Vector2Int? _selectionHighlightPos;

        private void OnEnable()
        {
            EventBus.Subscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Subscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Subscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);

            EventBus.Subscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Subscribe<UnitDeselectedEvent>(OnUnitDeselected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Unsubscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Unsubscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);

            EventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);
        }

        private void RenderTerrain(MapViewInitEvent mapViewInitEvent)
		{
            var mapData = mapViewInitEvent.MapData;
            groundTilemap.ClearAllTiles();

			foreach (var cell in mapData.Cells.Values)
            {
				if (cell.Tile)
                    groundTilemap.SetTile((Vector3Int)cell.Position, cell.Tile);
                if (cell.SceneActor != null && cell.SceneActor.BaseCell == cell)
                {
                    sceneActorTilemap.SetTile((Vector3Int)cell.Position, cell.SceneActor.Tile);
                }
			}

            foreach (var wall in mapData.Walls.Values)
            {
	            if (!wall.Tile) continue;

	            (Vector2Int pos, bool isLeft) wallKey = wall.Key.ToPositionAndIsLeft();
	            if (wallKey.isLeft)
		            leftWallTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
	            else
		            rightTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
            }
		}

		private void OnRangeDisplay(RangeDisplayEvent e)
		{
			if (e.Cells.Count == 0)
			{
				highlightTilemap.ClearAllTiles();
				return;
			}

			if (e is { RangeType: ERangeType.Movement, CellCosts: not null })
				ShowMovementRangeHighlight(e.Cells, e.CellCosts);
			else
				ShowRangeHighlight(e.Cells, e.RangeType);
		}

		private void OnUnitSelected(UnitSelectedEvent e)
		{
			SetTileWithColor(highlightTilemap, e.Position, highlightRuleTile, selectionHighlightColor);
			_selectionHighlightPos = e.Position;
		}

		private void OnUnitDeselected(UnitDeselectedEvent e) => highlightTilemap.ClearAllTiles();

		private void ShowRangeHighlight(IReadOnlyList<Vector2Int> cells, ERangeType rangeType)
		{
			highlightTilemap.ClearAllTiles();

			var color = GetRangeColor(rangeType);

			foreach (var pos in cells)
				SetTileWithColor(highlightTilemap, pos, highlightRuleTile, color);
		}

		private void ShowMovementRangeHighlight(
			IReadOnlyList<Vector2Int> cells,
			IReadOnlyDictionary<Vector2Int, int> cellCosts)
		{
			highlightTilemap.ClearAllTiles();

			foreach (var pos in cells)
			{
				int apCost = cellCosts != null && cellCosts.TryGetValue(pos, out var cost)
					? cost
					: 1;
				int colorIndex = Mathf.Clamp(apCost - 1, 0, moveApColors.Length - 1);
				SetTileWithColor(highlightTilemap, pos, highlightRuleTile, moveApColors[colorIndex]);
			}
		}

		private Color GetRangeColor(ERangeType rangeType)
		{
			return rangeType switch
			{
				ERangeType.Attack      => attackRangeColor,
				ERangeType.Skill       => attackRangeColor, // reuse for now
				ERangeType.AreaOfEffect => attackRangeColor,
				ERangeType.Movement    => moveApColors.Length > 0 ? moveApColors[0] : Color.blue,
				_ => Color.white
			};
		}

		private static void SetTileWithColor(Tilemap tilemap, Vector2Int pos, TileBase tile, Color color)
		{
			var pos3 = (Vector3Int)pos;
			tilemap.SetTile(pos3, tile);
			tilemap.SetTileFlags(pos3, TileFlags.None);
			tilemap.SetColor(pos3, color);
		}

		#region 墙体透明

		private Vector2Int? _previousHoverCellPos;

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue) return;

			List<MapWall> walls;
			if (_previousHoverCellPos.HasValue)
			{
				walls = MapService.GetWallsWhichHideCell(_previousHoverCellPos.Value);
				foreach (var wall in walls)
				{
					if (wall == null) continue;
					var targetColor = MapService.CheckWallTransparency(wall)
						? new Color(1, 1, 1, 0.5f)
						: Color.white;
					SetWallColor(wall, targetColor);
				}
			}

			walls = MapService.GetWallsWhichHideCell(e.CellPosition.Value);
			foreach (var wall in walls) SetWallColor(wall, new Color(1, 1, 1, 0.5f));

			_previousHoverCellPos = e.CellPosition;
		}

		private void OnMapCellChanged(MapCellChangedEvent e)
		{
			foreach (var wall in e.Walls)
			{
				if (wall == null) continue;
				var targetColor = MapService.CheckWallTransparency(wall)
					? new Color(1, 1, 1, 0.5f)
					: Color.white;
				SetWallColor(wall, targetColor);
			}
		}

		private void SetWallColor(MapWall wall, Color color)
		{
			if (wall == null) return;
			var targetTilemap = wall.Key.IsLeft() ? leftWallTilemap : rightTilemap;
			targetTilemap.SetTileFlags((Vector3Int)wall.Key.Position, TileFlags.None);
			targetTilemap.SetColor((Vector3Int)wall.Key.Position, color);
		}

		#endregion
	}
}
