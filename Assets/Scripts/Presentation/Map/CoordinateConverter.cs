using Systems.Interfaces;
using UnityEngine;

namespace Presentation.Map
{
	public class CoordinateConverter : ICoordinateConverter
	{
		private readonly Grid _grid;

		private readonly Vector2 _basisX;
		private readonly Vector2 _basisY;
		private readonly Vector2 _center00;

		public CoordinateConverter(Grid grid)
		{
			_grid = grid;
			_center00 = grid.GetCellCenterWorld(Vector3Int.zero);
			var center10 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
			var center01 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
			_basisX = center10 - _center00;
			_basisY = center01 - _center00;
		}

		public Vector2Int WorldToCell(Vector3 worldPosition)
			=> (Vector2Int)_grid.WorldToCell(worldPosition);

		public Vector3 CellToWorld(Vector2Int cellPosition)
			=> _grid.GetCellCenterWorld((Vector3Int)cellPosition);

		public (Vector2 basisX, Vector2 basisY) GetBasis() => (_basisX, _basisY);

		public Vector2 GetCenter00() => _center00;

		public Grid GetGrid() => _grid;
	}
}
