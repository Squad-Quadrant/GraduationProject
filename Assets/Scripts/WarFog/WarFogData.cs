using System.Collections.Generic;
using UnityEngine;

namespace WarFog
{
    // 我们认为战场是二维网格，每个格子可以有不同的迷雾状态，且迷雾状态会随当前角色，Buff或其他角色技能等因素动态变化
    // 每个网格游戏物体包含一个WarFog组件，负责管理该网格的迷雾状态和显示效果
    // WarFogData类用于存储和管理单个角色的迷雾数据，并且支持序列化以便保存和加载和运行时比较灵活的修改
    public class WarFogData
    {
        [SerializeField] private WarFogState[,] _visibilityGrids;

        // public WarFogState[,] VisibilityGrid
        // {
        //     get
        //     {
        //         // 深拷贝
        //         // var currentVisibilityGrids 
        //     }
        // }

        public List<WarFogAdjust> WarFogAdjusts = new();

    }
    
    public delegate void WarFogAdjust(WarFogState[,] currentVisibilityGrids);
}
