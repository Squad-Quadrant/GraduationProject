using System;
using Core.Events;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField] private Tilemap GroundTilemap;
        [SerializeField] private Tilemap LeftWallTilemap;
        [SerializeField] private Tilemap RightTilemap;
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = RootContainer.Instance.Resolve<IEventBus>();
        }

        private void OnEnable()
        {
            // _eventBus.Subscribe<MapViewInitEvent>(RenderTerrain);
        }

        private void OnDisable()
        {
            _eventBus.Unsubscribe<MapViewInitEvent>(RenderTerrain);
        }

		// todo: implement highlighting and indicators
		public void HighlightCells(Vector2Int[] positions, EHighlightType type)
		{
            
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
	}
}
