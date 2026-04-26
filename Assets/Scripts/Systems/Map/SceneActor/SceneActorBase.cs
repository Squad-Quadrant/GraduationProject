using System.Collections.Generic;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    // 场景物体的个体差异大，运行时需要定义子类，且配置数据到运行时数据的转换需要工厂，我们用枚举来区分场景物体的类型
    public abstract class SceneActorBase
    {
	    public uint Uid { get; }
	    public SceneActorConfig Config { get; }
        public MapCell BaseCell { get; }
        public IReadOnlyList<MapCell> ExtraCells { get; }

        public string DisplayName => Config.displayName;

        public bool BlocksVision { get; set; }
        public List<Vector2Int> BlockMovement { get; set; }

        public virtual IReadOnlyList<SpriteSlice> CurrentSlices => Config.baseSlices;

        
        public SceneActorBase(uint uid, SceneActorConfig config, MapCell baseCell, IReadOnlyList<MapCell> extraCells)
        {
	        Uid = uid;
	        Config = config;
	        BaseCell = baseCell;
	        ExtraCells = extraCells;

	        BlocksVision = config.blockVision;
	        BlockMovement = new List<Vector2Int>(config.blockMovementFrom);
        }
    }
}
