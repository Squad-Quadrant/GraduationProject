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
		[SerializeField] private Tilemap GroundTilemap;
        [SerializeField] private Tilemap LeftWallTilemap;
        [SerializeField] private Tilemap RightTilemap;
        [SerializeField] private Tilemap SceneActorTilemap;
        [SerializeField] private Tilemap HighlightTilemap;
        [SerializeField] private Tilemap UnitTilemap;
        [SerializeField] private Tile HighlightTile; // just a simple highlight tile for demonstration
        
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
            _eventBus.Subscribe<MapViewRenderUnitEvent>(RenderUnit);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (enableGenerate)
#endif
            _eventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
            _eventBus.Unsubscribe<RangeDisplayEvent>(DisplayHighlight);
            _eventBus.Unsubscribe<MapViewRenderUnitEvent>(RenderUnit);
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
            GroundTilemap.ClearAllTiles();

			foreach (var cell in mapData.Cells.Values)
            {
				if (cell.Tile)
                    GroundTilemap.SetTile((Vector3Int)cell.Position, cell.Tile);
                if (cell.SceneActor != null && cell.SceneActor.BaseCell == cell)
                {
                    SceneActorTilemap.SetTile((Vector3Int)cell.Position, cell.SceneActor.Tile);
                }
			}

            foreach (var wall in mapData.Walls.Values)
            {
                if (wall.Tile)
                {
                    (Vector2Int pos, bool isLeft) wallKey = wall.Key.ToPositionAndIsLeft();
                    if (wallKey.isLeft)
                    {
                        LeftWallTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
                    }
                    else
                    {
                        RightTilemap.SetTile((Vector3Int)wallKey.pos, wall.Tile);
                    }
                }
            }
		}

		private void DisplayHighlight(RangeDisplayEvent e)
		{
			if (e.Cells.Count == 0)
			{
				HighlightTilemap.ClearAllTiles();
				return;
			}

			foreach (var cellPos in e.Cells)
				HighlightTilemap.SetTile((Vector3Int)cellPos, HighlightTile);
		}

        private void RenderUnit(MapViewRenderUnitEvent e)
        {
            var units = e.UnitsToRender;
            UnitTilemap.ClearAllTiles();

            // todo: 考虑到某些没有UnitServer上下文的情况,也需要能够渲染单位, 因此提供一个AutoGetUnits的选项, 让MapView自己去UnitServer里拿需要渲染的单位列表
            // todo: 获得UnitServer里Unit的办法
            
            // if (e.AutoGetUnits)
            // {
            //     units = 
            // }
            
            foreach (var unit in units)
            {
                var pos = unit.Position;
                RuleTile tile = unit.ruleTile;
                UnitTilemap.SetTile((Vector3Int)pos, tile);
            }
            
        }
	}
}
