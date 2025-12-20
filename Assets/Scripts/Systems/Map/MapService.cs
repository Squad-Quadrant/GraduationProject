using Core.Log;
using Data.Config;
using UnityEngine;

namespace Systems.Map
{
	public class MapService : IMapService
	{
		public MapData Data { get; } = new();

		public MapService() => this.Log("Initialized");

		public void LoadFromConfig(MapConfig config)
		{
			Data.Initialize(config.Size);

			foreach (var cellConfig in config.cells)
			{
				var cell = Data.GetCell(cellConfig.position);
				if (cell == null) continue;
				cell.Terrain = cellConfig.Terrain;
				cell.IsWalkable = cellConfig.IsWalkable;
				cell.MoveCost = cellConfig.MoveCost;
				// cell.Height = cellConfig.height;
                cell.tile = cellConfig.cell?.Tile;
			}

			this.Log($"Loaded map '{config.MapName}' ({config.Size.x}x{config.Size.y})");
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
			// cell.IsOccupied = true;
			// cell.OccupantId = unitId;
		}

		public void ReleaseCell(Vector2Int position)
		{
			var cell = Data.GetCell(position);
			if (cell == null) return;
			// cell.IsOccupied = false;
			// cell.OccupantId = null;
		}
	}
}
