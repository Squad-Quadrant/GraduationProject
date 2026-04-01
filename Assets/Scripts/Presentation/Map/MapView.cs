using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Map;
using Systems.Map.Config;
using Systems.Map.Region;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
	[Serializable]
	public class PathTileConfig
	{
		public EPathSegmentType type;
		public TileBase tile;
	}

    /// <summary>
    /// MapView 目前，该组件承担渲染所有与地图相关的视觉元素的责任，包括地形、墙壁、单位和高亮显示等，而不只是地图。
    /// </summary>
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private Grid grid;
		[SerializeField, Required] private SpriteRenderer groundRenderer;
        [SerializeField, Required] private Tilemap leftWallTilemap;
        [SerializeField, Required] private Tilemap rightTilemap;
        [SerializeField, Required] private Tilemap sceneActorTilemap;
        [SerializeField, Required] private Tilemap highlightTilemap;
        [SerializeField, Required] private Tilemap pathTilemap;

        [Title("Highlight")]
        [SerializeField, Required] private RuleTile highlightRuleTile;
        [SerializeField] private Color selectionHighlightColor = new(1f, 1f, 0.6f, 0.65f);
        [SerializeField] private Color[] moveApColors = {
	        new(0.2f, 0.5f, 1.0f, 0.55f),  // AP 1
	        new(0.35f, 0.6f, 1.0f, 0.45f), // AP 2
	        new(0.5f, 0.7f, 1.0f, 0.35f),  // AP 3
	        new(0.65f, 0.8f, 1.0f, 0.25f), // AP 4
        };
        [SerializeField] private Color attackRangeColor = new(1f, 0.3f, 0.3f, 0.5f);

        [Title("Path Preview")]
        [SerializeField, TableList] private List<PathTileConfig> pathTileConfigs = new();
        
        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

        private IRegionService _regionService;
        private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

        private Dictionary<EPathSegmentType, TileBase> _pathTileDic;

        private void OnEnable()
        {
            EventBus.Subscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Subscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Subscribe<PathPreviewEvent>(OnPathPreview);
            EventBus.Subscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);

            EventBus.Subscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Subscribe<UnitDeselectedEvent>(OnUnitDeselected);

            EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);

            BuildPathTileDictionary();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Unsubscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Unsubscribe<PathPreviewEvent>(OnPathPreview);
            EventBus.Unsubscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);

            EventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);

            EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
        }

        private void RenderTerrain(MapViewInitEvent e)
		{
            var mapData = e.MapData;

            if (e.GroundSprite)
            {
	            groundRenderer.sprite = e.GroundSprite;
	            groundRenderer.transform.position = ComputeGridOrigin(mapData.Size);
            }
            else
	            this.LogWarning("No ground sprite assigned in MapConfig.");

            foreach (var cell in mapData.Cells.Values)
            {
                if (cell.SceneActor != null && cell.SceneActor.BaseCell == cell)
	                sceneActorTilemap.SetTile((Vector3Int)cell.Position, cell.SceneActor.Tile);
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

            SetInitialRegionVisibility(mapData);
		}

		private Vector3 ComputeGridOrigin(Vector2Int mapSize)
		{
			var center00 = (Vector2)grid.GetCellCenterWorld(Vector3Int.zero);
			var center10 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
			var center01 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
			Vector2 basisX = center10 - center00;
			Vector2 basisY = center01 - center00;

			Vector2 bottomPoint = center00 - 0.5f * basisX - 0.5f * basisY;
			Vector2 leftPoint = center00 - 0.5f * basisX + (mapSize.y - 0.5f) * basisY;
			return new Vector3(leftPoint.x, bottomPoint.y, 0f);
		}


		#region Highlight & Path Preview

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

		private void OnPathPreview(PathPreviewEvent e)
		{
			pathTilemap.ClearAllTiles();

			if (!e.IsValid || e.Path == null || e.Path.Count < 2)
				return;

			var segments = PathTileResolver.Resolve(e.Path);
			foreach (var (pos, segmentType) in segments)
			{
				if (!_pathTileDic.TryGetValue(segmentType, out var tile) || !tile)
				{
					this.LogWarning($"Can not get tile of type: {segmentType}");
					continue;
				}
				pathTilemap.SetTile((Vector3Int)pos, tile);
			}
		}

		private void BuildPathTileDictionary()
		{
			_pathTileDic = new Dictionary<EPathSegmentType, TileBase>();
			foreach (var config in pathTileConfigs)
			{
				if (_pathTileDic.ContainsKey(config.type))
				{
					this.LogWarning($"Path tile config type {config.type} dual config");
					continue;
				}
				_pathTileDic[config.type] = config.tile;
			}
		}

		private void OnUnitSelected(UnitSelectedEvent e) => SetTileWithColor(highlightTilemap, e.Position, highlightRuleTile, selectionHighlightColor);

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

		#endregion


		#region Wall Transparency

		private Vector2Int? _previousHoverCellPos;

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue) return;

			List<MapWall> walls;
			if (_previousHoverCellPos.HasValue)
			{
				walls = MapService.GetWallsWhichHideCell(_previousHoverCellPos.Value);
				foreach (var wall in walls
					         .Where(wall => wall != null)
					         .Where(wall => IsWallRegionVisible(wall.Key)))
				{
					SetWallAlpha(wall, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);
				}
			}

			walls = MapService.GetWallsWhichHideCell(e.CellPosition.Value);
			foreach (var wall in walls)
				SetWallAlpha(wall, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);
			_previousHoverCellPos = e.CellPosition;
		}

		private void OnMapCellChanged(MapCellChangedEvent e)
		{
			foreach (var wall in e.Walls)
			{
				if (wall == null) continue;
				SetWallAlpha(wall, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);
			}
		}

		#endregion


		#region Region Transparency

		private void OnRegionUnlocked(RegionUnlockedEvent e)
		{
			foreach (var cellPos in e.Cells)
			{
				var cell = MapService.Data.GetCell(cellPos);
				if (cell?.SceneActor == null || cell.SceneActor.BaseCell != cell)
					continue;
				SetSceneActorAlpha(cellPos, 1f);
			}

			foreach (var wallKey in e.BoundaryWalls)
			{
				var wall = MapService.Data.GetWall(wallKey);
				if (wall == null) continue;
				bool visible = IsWallRegionVisible(wallKey);
				SetWallAlpha(wall, visible ? 1f : 0);
			}

			foreach (var cellPos in e.Cells)
			{
				var cell = MapService.Data.GetCell(cellPos);
				if (cell == null) continue;

				var neighbors = new Vector2Int[]
				{
					new(cellPos.x + 1, cellPos.y),
					new(cellPos.x - 1, cellPos.y),
					new(cellPos.x, cellPos.y + 1),
					new(cellPos.x, cellPos.y - 1)
				};

				foreach (var neighbor in neighbors)
				{
					var wall = MapService.Data.GetWall(new WallKey(cellPos, neighbor));
					if (wall == null) continue;
					bool visible = IsWallRegionVisible(wall.Key);
					SetWallAlpha(wall, visible ? 1f : 0);
				}
			}
		}

		private void SetInitialRegionVisibility(MapData mapData)
		{
			foreach (var wall in mapData.Walls.Values)
			{
				if (wall == null) continue;
				bool visible = IsWallRegionVisible(wall.Key);
				SetWallAlpha(wall, visible ? 1f : 0f);
			}

			foreach (var cell in mapData.Cells.Values)
			{
				if (cell.SceneActor == null || cell.SceneActor.BaseCell != cell)
					continue;

				bool visible = RegionService.IsCellUnlocked(cell.Position);
				SetSceneActorAlpha(cell.Position, visible ? 1f : 0f);
			}
		}

		private bool IsWallRegionVisible(WallKey wallKey)
		{
			var (cellA, isLeft) = wallKey.ToPositionAndIsLeft();

			var cellB = isLeft
				? new Vector2Int(cellA.x, cellA.y + 1)
				: new Vector2Int(cellA.x + 1, cellA.y);

			return RegionService.IsCellUnlocked(cellA) || RegionService.IsCellUnlocked(cellB);
		}

		#endregion

		private void SetSceneActorAlpha(Vector2Int position, float alpha)
		{
			var pos3 = (Vector3Int)position;
			sceneActorTilemap.SetTileFlags(pos3, TileFlags.None);
			sceneActorTilemap.SetColor(pos3, new Color(1f, 1f, 1f, alpha));
		}

		private void SetWallAlpha(MapWall wall, float alpha)
		{
			if (wall == null) return;
			var targetTilemap = wall.Key.IsLeft() ? leftWallTilemap : rightTilemap;
			var targetColor = new Color(1f, 1f, 1f, alpha);
			targetTilemap.SetTileFlags((Vector3Int)wall.Key.Position, TileFlags.None);
			targetTilemap.SetColor((Vector3Int)wall.Key.Position, targetColor);
		}

		private static void SetTileWithColor(Tilemap tilemap, Vector2Int pos, TileBase tile, Color color)
		{
			var pos3 = (Vector3Int)pos;
			tilemap.SetTile(pos3, tile);
			tilemap.SetTileFlags(pos3, TileFlags.None);
			tilemap.SetColor(pos3, color);
		}
	}
}
