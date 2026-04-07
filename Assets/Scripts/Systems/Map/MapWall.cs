using System;
using JetBrains.Annotations;
using Systems.Map.Config;
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

    // 徽墨0206：构造时自动调整p1和p2的顺序，确保墙的唯一性
    public struct WallKey : IEquatable<WallKey>
    {
	    private Vector2Int _position1;
	    private Vector2Int _position2;

	    public WallKey(Vector2Int pos1, Vector2Int pos2)
	    {
		    if (pos1.x < pos2.x || (pos1.x == pos2.x && pos1.y < pos2.y))
		    {
			    _position1 = pos1;
			    _position2 = pos2;
		    }
		    else
		    {
			    _position1 = pos2;
			    _position2 = pos1;
		    }
	    }

	    public Vector2Int Position => _position1;

	    public (Vector2Int, bool) ToPositionAndIsLeft()
	    {
		    if (_position1.y == _position2.y)
			    return (_position1, false);

		    if (_position1.x == _position2.x)
			    return (_position1, true);

		    throw new ArgumentException("Positions do not form a valid wall.");
	    }

	    public bool IsLeft() => ToPositionAndIsLeft().Item2;

	    public bool Equals(WallKey other)
		    => _position1.Equals(other._position1) && _position2.Equals(other._position2);

	    public override bool Equals(object obj)
		    => obj is WallKey other && Equals(other);

	    public override int GetHashCode()
		    => HashCode.Combine(_position1, _position2);

	    public static bool operator ==(WallKey left, WallKey right) => left.Equals(right);
	    public static bool operator !=(WallKey left, WallKey right) => !left.Equals(right);

	    public override string ToString() => $"Wall[{_position1} <-> {_position2}]";
    }
    
    public class MapWall
    {
        public WallKey Key { get; }
        public WallType Type { get; set; }

        public MapWall(WallKey key)
        {
            Key = key;
        }
    }
}
