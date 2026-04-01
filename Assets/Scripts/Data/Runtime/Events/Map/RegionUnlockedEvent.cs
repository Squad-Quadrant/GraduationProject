using System.Collections.Generic;
using Core.Events;
using Systems.Map;
using UnityEngine;

namespace Data.Runtime.Events.Map
{
	public readonly struct RegionUnlockedEvent : IEvent
	{
		public readonly int RegionId;

		public readonly IReadOnlyList<Vector2Int> Cells;

		public readonly IReadOnlyList<WallKey> BoundaryWalls; // 不包含内部墙

		public RegionUnlockedEvent(int regionId, IReadOnlyList<Vector2Int> cells, IReadOnlyList<WallKey> boundaryWalls)
		{
			RegionId = regionId;
			Cells = cells;
			BoundaryWalls = boundaryWalls;
		}
	}
}
