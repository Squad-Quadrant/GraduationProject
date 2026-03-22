using System.Collections.Generic;
using UnityEngine;

namespace Presentation.Map
{
	internal enum EDirection
	{
		Right = 0,
		Left = 1,
		Up = 2,
		Down = 3
	}

	public enum EPathSegmentType
	{
		StraightX, StraightY, // 直线段
		TurnRightUp, TurnRightDown, TurnLeftUp, TurnLeftDown, // 拐弯段
		StartRight, StartLeft, StartUp, StartDown, // 起始段
		EndRight, EndLeft, EndUp, EndDown // 结束段 (箭头指向)
	}

	public static class PathTileResolver
	{
		public static List<(Vector2Int pos, EPathSegmentType type)> Resolve(IReadOnlyList<Vector2Int> path)
		{
			if (path == null || path.Count < 2) return null;

			var result = new List<(Vector2Int pos, EPathSegmentType type)>();

			var startDir = GetDirection(path[0], path[1]);
			var startSegmentType = startDir switch
			{
				EDirection.Right => EPathSegmentType.StartRight,
				EDirection.Left => EPathSegmentType.StartLeft,
				EDirection.Up => EPathSegmentType.StartUp,
				EDirection.Down => EPathSegmentType.StartDown,
				_ => EPathSegmentType.EndRight // 随便给一个错误的值
			};
			result.Add((path[0], startSegmentType));

			for (int i = 1; i < path.Count - 1; i++)
			{
				var dir1 = GetDirection(path[i], path[i - 1]);
				var dir2 = GetDirection(path[i], path[i + 1]);
				if (dir1 > dir2) (dir1, dir2) = (dir2, dir1);
				var segmentType = (dir1, dir2) switch
				{
					(EDirection.Right, EDirection.Left) => EPathSegmentType.StraightX,
					(EDirection.Right, EDirection.Up) => EPathSegmentType.TurnRightUp,
					(EDirection.Right, EDirection.Down) => EPathSegmentType.TurnRightDown,
					(EDirection.Left, EDirection.Up) => EPathSegmentType.TurnLeftUp,
					(EDirection.Left, EDirection.Down) => EPathSegmentType.TurnLeftDown,
					(EDirection.Up, EDirection.Down) => EPathSegmentType.StraightY,
					_ => startSegmentType // 随便给一个错误的值
				};
				result.Add((path[i], segmentType));
			}

			var endDir = GetDirection(path[^2], path[^1]);
			var endSegmentType = endDir switch
			{
				EDirection.Right => EPathSegmentType.EndRight,
				EDirection.Left => EPathSegmentType.EndLeft,
				EDirection.Up => EPathSegmentType.EndUp,
				EDirection.Down => EPathSegmentType.EndDown,
				_ => EPathSegmentType.StartRight // 随便给一个错误的值
			};
			result.Add((path[^1], endSegmentType));

			return result;
		}

		private static EDirection GetDirection(Vector2Int from, Vector2Int to)
		{
			var delta = to - from;
			if (delta == Vector2Int.right) return EDirection.Right;
			if (delta == Vector2Int.left)  return EDirection.Left;
			if (delta == Vector2Int.up)    return EDirection.Up;
			if (delta == Vector2Int.down)  return EDirection.Down;
			Debug.LogWarning("[PathTileResolver] Invalid Direction");
			return EDirection.Right;
		}
	}
}
