using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Presentation.Map.PathPreview;
using Presentation.Map.Wall;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.Map.Region;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private SpriteRenderer groundRenderer;
        [SerializeField, Required] private Tilemap sceneActorTilemap;
        [SerializeField, Required] private Tilemap cursorHoverTilemap;

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
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
            EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);
        }

        private void OnDisable()
        {
	        if (!LevelContainer.Instance) return;
            EventBus.Unsubscribe<MapViewInitEvent>(InitMap);
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
            EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
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

            // 初始化场景物体 tile
            foreach (var cell in mapData.Cells.Values)
            {
                if (cell.SceneActor != null && cell.SceneActor.BaseCell == cell)
	                sceneActorTilemap.SetTile((Vector3Int)cell.Position, cell.SceneActor.Tile);
            }

            // 按区域解锁状态设置场景物体透明度
            foreach (var cell in mapData.Cells.Values)
            {
	            if (cell.SceneActor == null || cell.SceneActor.BaseCell != cell)
		            continue;

	            bool visible = RegionService.IsCellUnlocked(cell.Position);
	            SetSceneActorAlpha(cell.Position, visible ? 1f : 0f);
            }

            // 实例化墙 prefab
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

		private void OnPointerHover(PointerHoverEvent e)
		{
			cursorHoverTilemap.ClearAllTiles();
			if (!e.CellPosition.HasValue) return;
			if (e.HoveredUnitId != null) return;
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
	}
}
