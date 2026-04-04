using UnityEngine;

namespace Systems.Interfaces
{
	public interface ICoordinateConverter
	{
		Vector2Int WorldToCell(Vector3 worldPosition);
		Vector3 CellToWorld(Vector2Int cellPosition);

		(Vector2 basisX, Vector2 basisY) GetBasis();

		Vector2 GetCenter00();

		Grid GetGrid();
	}
}
