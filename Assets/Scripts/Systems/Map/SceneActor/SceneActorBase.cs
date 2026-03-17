using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    // 场景物体的个体差异大，运行时需要定义子类，且配置数据到运行时数据的转换需要工厂，我们用枚举来区分场景物体的类型
    public abstract class SceneActorBase
    {
        protected SceneActorType _type;
        public SceneActorType Type => _type;
        protected uint _uid;
        public uint Uid => _uid;
        
        public MapCell BaseCell { get; set; }

        public List<MapCell> ExtraCells { get; set; } = new();
        
        public Tile Tile { get; set; }

        public bool BlocksVision { get; set; }
        
        public SceneActorBase(SceneActorType type, uint uid)
        {
            _type = type;
            _uid = uid;
        }
    }

    public enum SceneActorType
    {
        Box,
        Container, // 集装箱
        Forklift,
        WeaponCabinet
    }
}
