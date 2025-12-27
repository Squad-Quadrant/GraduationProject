using System;
using Data.Config.Map;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map
{
    // 樯是两个格子之间的东西,所以墙的唯一ID用两个坐标标识
    // 这样的表示有利于查询和管理，应用于配置和"运行时系统"
    // 在“运行时表现"，我们用一个坐标和左右标识墙
    
    // 我们用两种方式来标识墙
    // 1. 两个坐标: 即墙之间的两个格子的坐标，用于数据配置和查询
    // 2. 一个坐标和左右: 即墙所在格子的坐标和墙在该格子的左侧还是右侧，用于运行时表现
    public struct WallKey : IEquatable<WallKey>
    {
        public Vector2Int position1;
        public Vector2Int position2;
    
        public WallKey(Vector2Int pos1, Vector2Int pos2)
        {
            position1 = pos1;
            position2 = pos2;
        }
        
        public (Vector2Int, bool) ToPositionAndIsLeft()
        {
            if (position1.x == position2.x)
            {
                if (position1.y < position2.y)
                {
                    return (position1, false);
                }
                return (position2, true);
            }

            if (position1.y == position2.y)
            {
                if (position1.x < position2.x)
                {
                    return (position1, true);
                }
                return (position2, false);
            }
            throw new ArgumentException("Positions do not form a valid wall.");
        }

        public bool IsLeft()
        {
            return ToPositionAndIsLeft().Item2;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is WallKey other)
            {
                return (position1 == other.position1 && position2 == other.position2) ||
                       (position1 == other.position2 && position2 == other.position1);
            }
            return false;
        }

        public bool Equals(WallKey other)
        {
            return position1.Equals(other.position1) && position2.Equals(other.position2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position1, position2);
        }
    }
    
    public class MapWall
    {
        public WallKey Key { get; }
        public WallType WallType { get; set; }
        [CanBeNull] public TileBase Tile { get; set; }

        public MapWall(WallKey key)
        {
            Key = key;
        }
    }
}