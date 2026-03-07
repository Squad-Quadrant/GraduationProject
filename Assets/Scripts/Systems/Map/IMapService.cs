using System.Collections.Generic;
using Data.Config;
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

		List<MapWall> GetWallsWhichHideCell(Vector2Int cellPos); // 得到可能会遮挡该格子的墙

		bool CheckWallTransparency(MapWall wall); // 检查墙应不应该半透
	}
}
