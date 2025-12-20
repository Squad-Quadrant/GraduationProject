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
			RightTilemap.ClearAllTiles();

			foreach (var cell in mapData.Cells.Values)
            {
				if (cell.tile)
					RightTilemap.SetTile((Vector3Int)cell.Position, cell.tile);
			}
		}
	}
}
