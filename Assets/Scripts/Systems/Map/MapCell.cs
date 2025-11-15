using UnityEngine;

namespace Systems.Map
{
	/// <summary>
	/// Runtime representation of a single cell in the map grid.
	/// </summary>
	public class MapCell
	{
		public Vector2Int Position { get; }
		public ETerrainType Terrain { get; set; }
		public int Height { get; set; } = 0;
		public bool IsWalkable { get; set; } = true;
		public int MoveCost { get; set; } = 1;
		public bool IsOccupied { get; set; } = false; // Indicates if an entity is currently occupying the cell
		public string OccupantId { get; set; }

		public MapCell(Vector2Int position) => Position = position;
	}
}
