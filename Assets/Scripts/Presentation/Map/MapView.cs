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

		// [Title("Tiles")]
		// [SerializeField] private TileBase[] tiles;

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

		public void RenderTerrain(MapData mapData)
		{
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
