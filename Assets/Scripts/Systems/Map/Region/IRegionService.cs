using System.Collections.Generic;
using Systems.Map.Config;
using UnityEngine;

namespace Systems.Map.Region
{
	public interface IRegionService
	{
		/// <summary>
		/// 某个regionId有没有解锁
		/// </summary>
		bool IsRegionUnlocked(int regionId);

		bool IsCellUnlocked(Vector2Int position);

		void UnlockRegion(int regionId);

		IReadOnlyList<Vector2Int> GetRegionCells(int regionId);

		IReadOnlyList<WallKey> GetRegionBoundaryWalls(int regionId);

		void Initialize(MapConfig config);
	}
}
