using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Map.Config;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    public class SceneActorFactory
    {
        private uint _count; // 用于生成唯一ID的计数器，如果不希望id重复，需要保证场景物体都由同一个工厂创建
        public SceneActorBase CreateSceneActor(SceneActorConfig config, MapData map, MapCell baseCell)
        {
            _count++;
            uint uid = _count;

            Tile tile = config.tiles[UnityEngine.Random.Range(0, config.tiles.Count)];
            List<MapCell> extraCells = config.extraGrid
	            .Select(offset => baseCell.Position + offset)
	            .Select(map.GetCell)
	            .Where(extraCell => extraCell != null).ToList();

            SceneActorBase sceneActor = config.type switch
            {
	            SceneActorType.Normal => new GeneralSceneActor(config.type, uid, tile, baseCell, extraCells),
	            SceneActorType.Door   => new DoorSceneActor(config.type, uid, tile, baseCell, extraCells, config.regionId),
	            _ => throw new NotImplementedException()
            };
            sceneActor.BlocksVision = config.blockVision;
			sceneActor.BlockMovement = config.blockMovementFrom;
			sceneActor.DisplayName = config.displayName;
			return sceneActor;
        }
    }
}
