using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Systems.Map.Config;
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
			}
            
            foreach (var wallConfig in config.walls)
            {
                var wall = Data.GetWall(wallConfig.WallKey);
                if (wall == null) continue;
                wall.Type = wallConfig.WallType;
            }
            
            var sceneActorFactory = new SceneActorFactory();
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
            
            _eventBus.Publish(new MapViewInitEvent(Data, config.groundSprite, config.wallViewPrefab));
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
		}

		public void ReleaseCell(Vector2Int position)
		{
			var cell = Data.GetCell(position);
			if (cell == null) return;
			cell.UnitId = null;
		}
	}
}
