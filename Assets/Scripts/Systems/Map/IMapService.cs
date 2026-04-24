using System.Collections.Generic;
using Data.Config;
using Systems.Map.Config;
using UnityEngine;

namespace Systems.Map
{
	public interface IMapService
	{
		MapData Data { get; }

		void LoadFromConfig(MapConfig config);

		bool IsCellWalkable(Vector2Int position);

		void OccupyCell(Vector2Int position, string unitId);

		void ReleaseCell(Vector2Int position);
	}
}
