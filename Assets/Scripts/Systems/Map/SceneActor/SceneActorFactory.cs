using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    public class SceneActorFactory
    {
        private uint _count; // 用于生成唯一 ID 的计数器

        public SceneActorBase CreateSceneActor(SceneActorConfig config, MapData map, MapCell baseCell)
        {
            _count++;
            uint uid = _count;

            List<MapCell> extraCells = config.extraGrid
	            .Select(offset => map.GetCell(baseCell.Position + offset))
	            .Where(c => c != null)
	            .ToList();

            return config switch
            {
	            DoorSceneActorConfig doorConfig => new DoorSceneActor(uid, doorConfig, baseCell, extraCells),
	            _								=> new GeneralSceneActor(uid, config, baseCell, extraCells)
            };
        }
    }
}
