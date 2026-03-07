using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events.Map;
using Systems.Map.SceneActor;
using UnityEngine;

namespace Systems.Map
{
	public class MapService : IMapService
	{
		public MapData Data { get; } = new();
        
        private readonly IEventBus _eventBus;

        public MapService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

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
                cell.Tile = cellConfig.cell?.Tile;
			}
            
            foreach (var wallConfig in config.walls)
            {
                var wall = Data.GetWall(wallConfig.WallKey);
                if (wall == null) continue;
                wall.WallType = wallConfig.WallType;
                wall.Tile = wallConfig.WallKey.IsLeft() ? wallConfig.wall?.leftTile : wallConfig.wall?.rightTile;
            }
            
            var sceneActorFactory = new SceneActorFactory();
            // 场景物体的初始化逻辑需要滞后于地图单元格的初始化
            foreach (var cellConfig in config.cells)
            {
                var cell = Data.GetCell(cellConfig.position);
                if (cell == null) continue;
                
                if (!cellConfig.sceneActor) continue;
                
                cell.SceneActor = sceneActorFactory.CreateSceneActor(cellConfig.sceneActor, Data, cell);
                foreach (var extraCell in cell.SceneActor.ExtraCells)
                {
                    extraCell.SceneActor = cell.SceneActor;
                }
            }
            
			this.Log($"Loaded map '{config.MapName}' ({config.Size.x}x{config.Size.y})");
            
            _eventBus.Publish(new MapViewInitEvent(Data));
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
			cell.UnitId = unitId;
			var pos = cell.Position;
			var walls = GetWallsWhichHideCell(pos);
			_eventBus.Publish(new MapCellChangedEvent(cell, walls));
		}

		public void ReleaseCell(Vector2Int position)
		{
			var cell = Data.GetCell(position);
			if (cell == null) return;
			cell.UnitId = null;
			var pos = cell.Position;
			var walls = GetWallsWhichHideCell(pos);
			_eventBus.Publish(new MapCellChangedEvent(cell, walls));
		}

		public List<MapWall> GetWallsWhichHideCell(Vector2Int cellPos) // 得到可能会影响给定格子视觉效果的墙，用于墙的半透效果
		{
			return new List<MapWall>
			{
				Data.GetWall(new WallKey(cellPos, new Vector2Int(cellPos.x - 1, cellPos.y))),
				Data.GetWall(new WallKey(cellPos, new Vector2Int(cellPos.x    , cellPos.y - 1))),
				Data.GetWall(new WallKey(new Vector2Int(cellPos.x - 1, cellPos.y),     new Vector2Int(cellPos.x - 1, cellPos.y - 1))),
				Data.GetWall(new WallKey(new Vector2Int(cellPos.x    , cellPos.y - 1), new Vector2Int(cellPos.x - 1, cellPos.y - 1)))
			};
		}
	}
}
