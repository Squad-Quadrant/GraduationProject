using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Presentation.Map.Wall;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
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
		[SerializeField, Required] private SpriteRenderer groundRenderer;
        [SerializeField, Required] private Tilemap sceneActorTilemap;
        [SerializeField, Required] private Tilemap highlightTilemap;
        [SerializeField, Required] private Tilemap pathTilemap;
        [SerializeField, Required] private Tilemap cursorHoverTilemap;

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
        [SerializeField] private Color interactRangeColor = new(1f, 0.8f, 0.3f, 0.5f);

        [Title("Path Preview")]
        [SerializeField, TableList] private List<PathTileConfig> pathTileConfigs = new();

        [Title("Cursor Hover")]
        [SerializeField] private TileBase cursorHoverTile;

        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

        private ICoordinateConverter _coordinateConverter;
		private ICoordinateConverter CoordinateConverter => _coordinateConverter ??= LevelContainer.Instance.Resolve<ICoordinateConverter>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

        private IRegionService _regionService;
        private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

        private Dictionary<EPathSegmentType, TileBase> _pathTileDic;

        private WallViewManager _wallViewManager;

        private void OnEnable()
        {
            EventBus.Subscribe<MapViewInitEvent>(InitMap);
            EventBus.Subscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Subscribe<PathPreviewEvent>(OnPathPreview);
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
            EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Subscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Subscribe<UnitDeselectedEvent>(OnUnitDeselected);

            BuildPathTileDictionary();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapViewInitEvent>(InitMap);
            EventBus.Unsubscribe<RangeDisplayEvent>(OnRangeDisplay);
            EventBus.Unsubscribe<PathPreviewEvent>(OnPathPreview);
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
            EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
            EventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);
        }

        private void InitMap(MapViewInitEvent e)
		{
            var mapData = e.MapData;

            if (e.GroundSprite)
            {
	            groundRenderer.sprite = e.GroundSprite;
	            groundRenderer.transform.position = ComputeGridOrigin(mapData.Size);
            }
            else
	            this.LogWarning("No ground sprite assigned in MapConfig.");

            // init scene actor
            foreach (var cell in mapData.Cells.Values)
            {
                if (cell.SceneActor != null && cell.SceneActor.BaseCell == cell)
	                sceneActorTilemap.SetTile((Vector3Int)cell.Position, cell.SceneActor.Tile);
            }

            foreach (var cell in mapData.Cells.Values)
            {
	            if (cell.SceneActor == null || cell.SceneActor.BaseCell != cell)
		            continue;

	            bool visible = RegionService.IsCellUnlocked(cell.Position);
	            SetSceneActorAlpha(cell.Position, visible ? 1f : 0f);
            }

            // init wall view manager
            if (_wallViewManager) Destroy(_wallViewManager.gameObject);

            var wallPrefab = e.WallVisualsPrefab;
            if (!wallPrefab)
            {
	            this.LogWarning("No wall visuals prefab in MapConfig. Walls will not render.");
	            return;
            }

            var instance = Instantiate(wallPrefab, transform);
            instance.name = wallPrefab.name;

            _wallViewManager = instance.GetComponent<WallViewManager>();
            if (!_wallViewManager)
            {
	            this.LogWarning("Wall prefab is missing WallView component.");
	            Destroy(instance);
	            return;
            }
            _wallViewManager.Initialize(mapData);
		}

		private Vector3 ComputeGridOrigin(Vector2Int mapSize)
		{
			var (basisX, basisY) = CoordinateConverter.GetBasis();
			var center00 = CoordinateConverter.GetCenter00();

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

		private void OnUnitSelected(UnitSelectedEvent e) => SetTileWithColor(highlightTilemap, e.Position, highlightRuleTile, selectionHighlightColor);

		private void OnUnitDeselected(UnitDeselectedEvent e) => highlightTilemap.ClearAllTiles();

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
				ERangeType.Interact		=> interactRangeColor,
				_ => Color.white
			};
		}

		#endregion

		private void OnPointerHover(PointerHoverEvent e)
		{
			cursorHoverTilemap.ClearAllTiles();
			if (!e.CellPosition.HasValue) return;
			cursorHoverTilemap.SetTile((Vector3Int)e.CellPosition.Value, cursorHoverTile);
		}

		private void OnRegionUnlocked(RegionUnlockedEvent e)
		{
			foreach (var cellPos in e.Cells)
			{
				var cell = MapService.Data.GetCell(cellPos);
				if (cell?.SceneActor == null || cell.SceneActor.BaseCell != cell)
					continue;
				SetSceneActorAlpha(cellPos, 1f);
			}
		}

		private void SetSceneActorAlpha(Vector2Int position, float alpha)
		{
			var pos3 = (Vector3Int)position;
			sceneActorTilemap.SetTileFlags(pos3, TileFlags.None);
			sceneActorTilemap.SetColor(pos3, new Color(1f, 1f, 1f, alpha));
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
