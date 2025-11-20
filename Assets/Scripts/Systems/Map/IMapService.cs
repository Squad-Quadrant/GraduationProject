using Data.Config;
using UnityEngine;

namespace Systems.Map
{
	public interface IMapService
	{
		MapData Data { get; }

		void LoadFromConfig(MapConfig config);

		public bool IsCellWalkable(Vector2Int position);

		public void OccupyCell(Vector2Int position, string unitId);

		public void ReleaseCell(Vector2Int position);
	}
}
