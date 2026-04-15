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
            List<MapCell> extraCells = config.ExtraGrid
	            .Select(offset => baseCell.Position + offset)
	            .Select(map.GetCell)
	            .Where(extraCell => extraCell != null).ToList();

            switch (config.Type)
            {
                case SceneActorType.Box:
                case SceneActorType.Container:
                case SceneActorType.Forklift:
                case SceneActorType.WeaponCabinet:
                    // 随机换皮
                    var sceneActor = new GeneralSceneActor(config.Type, uid, tile, baseCell, extraCells)
                    {
	                    BlocksVision = config.blockVision,
	                    BlockMovement = config.blockMovement
                    };
                    return sceneActor;

                case SceneActorType.Door:
	                var door = new DoorSceneActor(config.Type, uid, tile, baseCell, extraCells, config.regionId)
	                {
		                BlocksVision = config.blockVision,
		                BlockMovement = config.blockMovement
	                };
	                return door;
                default:
                    throw  new NotImplementedException();
            }
        }
    }
}
