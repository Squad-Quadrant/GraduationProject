using Sirenix.OdinInspector;
using Systems.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField] private Tilemap terrainTilemap;

		[Title("Tiles")]
		[SerializeField] private TileBase[] tiles;

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
			terrainTilemap.ClearAllTiles();

			foreach (var cell in mapData.Cells.Values)
			{
				var tile = GetTileForTerrain(cell.Terrain);
				if (tile != null)
					terrainTilemap.SetTile((Vector3Int)cell.Position, tile);
			}
		}

		private TileBase GetTileForTerrain(ETerrainType terrainType)
		{
			// todo: implement tile retrieval based on terrain type
			return terrainType switch
			{
				ETerrainType.Plain => null,
				ETerrainType.Forest => null,
				_ => null
			};
		}
	}
}
