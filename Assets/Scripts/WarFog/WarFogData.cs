using System;
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
        
        
        public WarFogState[,] VisibilityGrids
        {
            get
            {
                if (_visibilityGrids == null)
                    return null;

                int rows = _visibilityGrids.GetLength(0);
                int cols = _visibilityGrids.GetLength(1);
                var data = new WarFogState[rows, cols];

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                        data[r, c] = _visibilityGrids[r, c];
                }

                foreach (var adjust in WarFogAdjusts)
                {
                    adjust?.Invoke(data);
                }

                return data;
            }
        }
        
        public void Init(WarFogState[,] visibilityGrids)
        {
            // todo: 我还没想好WarFogData本身序列化还是另起一数据类
            _visibilityGrids = visibilityGrids;
        }

        // 外部添加的视野调整
        public List<WarFogAdjust> WarFogAdjusts = new();
        
        public void AddAdjust(WarFogAdjust adjust)
        {
            WarFogAdjusts.Add(adjust);
        }

    }
    
    public delegate void WarFogAdjust(WarFogState[,] currentVisibilityGrids);
}
