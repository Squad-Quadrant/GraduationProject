using Systems.Interfaces;
using UnityEngine;

namespace Presentation.Map
{
	public class CoordinateConverter : ICoordinateConverter
	{
		private readonly Grid _grid;

		public CoordinateConverter(Grid grid) => _grid = grid;

		public Vector2Int WorldToCell(Vector3 worldPosition)
			=> (Vector2Int)_grid.WorldToCell(worldPosition);

		public Vector3 CellToWorld(Vector2Int cellPosition)
			=> _grid.GetCellCenterWorld((Vector3Int)cellPosition);
	}
}
