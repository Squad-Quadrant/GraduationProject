using Data.Config;
using UnityEngine;

namespace Systems.Map
{
	public class MapService : IMapService
	{
		public MapData Data { get; }

		public MapService(MapData mapData)
		{
			Data = mapData;
			Debug.Log("[MapService] Initialized");
		}

		public void LoadFromConfig(MapConfig config)
		{
			Data.Initialize(config.size);

			foreach (var cellConfig in config.cells)
			{
				var cell = Data.GetCell(cellConfig.position);
				if (cell == null) continue;
				cell.Terrain = cellConfig.terrain;
				cell.IsWalkable = cellConfig.isWalkable;
				cell.MoveCost = cellConfig.moveCost;
				cell.Height = cellConfig.height;
			}

			Debug.Log($"[MapService] Loaded map '{config.mapName}' ({config.size.x}x{config.size.y})");
		}

		public bool IsCellWalkable(Vector2Int position)
		{
			var cell = Data.GetCell(position);
			return cell is { IsWalkable: true, IsOccupied: false };
		}

		public void OccupyCell(Vector2Int position, string unitId)
		{
			var cell = Data.GetCell(position);
			if (cell == null) return;
			cell.IsOccupied = true;
			cell.OccupantId = unitId;
		}

		public void ReleaseCell(Vector2Int position)
		{
			var cell = Data.GetCell(position);
			if (cell == null) return;
			cell.IsOccupied = false;
			cell.OccupantId = null;
		}
	}
}
