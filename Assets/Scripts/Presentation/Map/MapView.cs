using Core.Events;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
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
        
#if UNITY_EDITOR
        public bool enableGenerate = true;
#endif
        private void Awake()
        {
            _eventBus = RootContainer.Instance.Resolve<IEventBus>();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (enableGenerate)
#endif
            _eventBus.Subscribe<MapViewInitEvent>(RenderTerrain);
            _eventBus.Subscribe<RangeDisplayEvent>(DisplayHighlight);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (enableGenerate)
#endif
            _eventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
            _eventBus.Unsubscribe<RangeDisplayEvent>(DisplayHighlight);
        }

		public void ClearHighlights()
		{
            
		}

		public void ShowCellIndicator(Vector2Int position, EIndicatorType type)
		{
            
		}

		public void HideCellIndicator()
		{
            
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
	}
}
