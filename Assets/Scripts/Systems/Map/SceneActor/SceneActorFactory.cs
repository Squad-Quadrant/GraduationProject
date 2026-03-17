using System;
using System.Collections.Generic;
using Data.Config;
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
            switch (config.Type)
            {
                case SceneActorType.Box:
                case SceneActorType.Container:
                case SceneActorType.Forklift:
                case SceneActorType.WeaponCabinet:
                    // 随机换皮 
                    Tile tile = config.tiles[UnityEngine.Random.Range(0, config.tiles.Count)];
                    List<MapCell> extraCells = new();
                    foreach (var offset in config.ExtraGrid)
                    {
                        var extraPos = baseCell.Position + offset;
                        var extraCell = map.GetCell(extraPos);
                        if (extraCell != null)
                        {
                            extraCells.Add(extraCell);
                        }
                    }
                    var sceneActor = new GeneralSceneActor(config.Type, uid, tile, baseCell, extraCells);
                    sceneActor.BlocksVision = config.blockVision;
                    return sceneActor;
                default:
                    throw  new NotImplementedException();
            }
        }
    }
}
