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
		[SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap leftWallTilemap;
        [SerializeField] private Tilemap rightTilemap;
        [SerializeField] private Tilemap sceneActorTilemap;
        [SerializeField] private Tilemap highlightTilemap;
        [SerializeField] private Tile highlightTile; // just a simple highlight tile for demonstration
        
        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();
        
#if UNITY_EDITOR
        public bool enableGenerate = true;
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (enableGenerate)
#endif
            EventBus.Subscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Subscribe<RangeDisplayEvent>(DisplayHighlight);
            EventBus.Subscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (enableGenerate)
#endif
            EventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
            EventBus.Unsubscribe<RangeDisplayEvent>(DisplayHighlight);
            EventBus.Unsubscribe<MapCellChangedEvent>(OnMapCellChanged);
            EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
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
                if (wall.Tile)
                {
                    (Vector2Int pos, bool isLeft) wallKey = wall.Key.ToPositionAndIsLeft();
                    if (wallKey.isLeft)
                    {
                        leftWallTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
                    }
                    else
                    {
                        rightTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
                    }
                }
            }
		}

		private void DisplayHighlight(RangeDisplayEvent e)
		{
			if (e.Cells.Count == 0)
			{
				highlightTilemap.ClearAllTiles();
				return;
			}

			foreach (var cellPos in e.Cells)
				highlightTilemap.SetTile((Vector3Int)cellPos, highlightTile);
		}

		private Vector2Int? _previousHoverCellPos;

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue) return;

			List<MapWall> walls;
			if (_previousHoverCellPos.HasValue)
			{
				walls = MapService.GetWallsWhichHideCell(_previousHoverCellPos.Value);
				foreach (var wall in walls) SetWallColor(wall, Color.white);
			}

			walls = MapService.GetWallsWhichHideCell(e.CellPosition.Value);
			foreach (var wall in walls) SetWallColor(wall, new Color(1, 1, 1, 0.5f));

			_previousHoverCellPos = e.CellPosition;
		}

		private void OnMapCellChanged(MapCellChangedEvent e)
		{
			var targetColor = e.Cell.IsOccupied ? new Color(1, 1, 1, 0.5f) : Color.white;
			foreach (var wall in e.Walls) SetWallColor(wall, targetColor);
		}

		private void SetWallColor(MapWall wall, Color color)
		{
			if (wall == null) return;

			var targetTilemap = wall.Key.IsLeft() ? leftWallTilemap : rightTilemap;
			targetTilemap.SetTileFlags((Vector3Int)wall.Key.Position, TileFlags.None);
			targetTilemap.SetColor((Vector3Int)wall.Key.Position, color);
		}
	}
}
