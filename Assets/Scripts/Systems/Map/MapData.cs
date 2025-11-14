using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems.Map
{
	/// <summary>
	/// Runtime representation of the map data structure.
	/// </summary>
	public class MapData
	{
		private readonly Dictionary<Vector2Int, MapCell> _cells = new();

		public Vector2Int Size { get; private set; } // Map Size
		public IReadOnlyDictionary<Vector2Int, MapCell> Cells => _cells; // Read-only access to map cells

		public void Initialize(Vector2Int size)
		{
			Size = size;
			_cells.Clear();

			for (int x = 0; x < size.x; x++)
			{
				for (int y = 0; y < size.y; y++)
				{
					var position = new Vector2Int(x, y);
					_cells[position] = new MapCell(position);
				}
			}
		}

		public MapCell GetCell(Vector2Int position) => _cells.GetValueOrDefault(position);

		public bool IsInBounds(Vector2Int position) =>
			position.x >= 0 && position.x < Size.x &&
			position.y >= 0 && position.y < Size.y;

		public List<MapCell> GetNeighbors(Vector2Int position)
		{
			var directions = new Vector2Int[]
			{
				new(0, 1),   // Up
				new(1, 0),   // Right
				new(0, -1),  // Down
				new(-1, 0)   // Left
			};

			return directions
				.Select(dir => position + dir)
				.Select(GetCell)
				.Where(neighborCell => neighborCell != null).ToList();
		}
	}
}
