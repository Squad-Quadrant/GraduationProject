using System.Collections.Generic;
using Systems.Map.SceneActor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map.Config
{
    [CreateAssetMenu(fileName = "SceneActorConfig", menuName = "Game/SceneActorConfig")]
    public class SceneActorConfig : ScriptableObject
    {
        public SceneActorType Type;
        public List<Vector2Int> ExtraGrid = new(); // 如果场景物体有多个格子占位，可以在这里配置额外的格子位置，位置相对于主格子的位置偏移
        // 有些场景物体有换皮，可以在这里配置不同皮肤对应的Tile，至于不同方向的物体，可以在运行时添加朝向变量，但在配置里不添加逻辑，而是直接使用多个SceneActorConfig实现
        public List<Tile> tiles = new();
    }
}
