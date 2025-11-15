using UnityEngine;

namespace Systems.Interfaces
{
	public interface ICoordinateConverter
	{
		Vector2Int WorldToCell(Vector3 worldPosition);
		Vector3 CellToWorld(Vector2Int cellPosition);
	}
}
