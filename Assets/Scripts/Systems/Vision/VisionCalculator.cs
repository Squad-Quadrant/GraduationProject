using System;
using System.Collections.Generic;
using Core.Log;
using Systems.Map;
using Systems.Map.Config;
using Systems.Unit;
using UnityEngine;

namespace Systems.Vision
{
	public class VisionCalculator : IVisionCalculator
	{
		private readonly IMapService _mapService;

		public VisionCalculator(IMapService mapService, IUnitService unitService) =>
			_mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));

		// 按曼哈顿距离遍历所有visionRange中的格子，一个个调用HasLineOfSight
		public HashSet<Vector2Int> CalculateVisibleCells(Vector2Int origin, int visionRange, List<Vector2Int> ignoredCells = null)
		{
			var visible = new HashSet<Vector2Int> { origin };
			var mapData = _mapService.Data;

			for (int dx = -visionRange; dx <= visionRange; dx++)
			{
				int maxDy = visionRange - Math.Abs(dx);
				for (int dy = -maxDy; dy <= maxDy; dy++)
				{
					if (dx == 0 && dy == 0) continue;

					var target = new Vector2Int(origin.x + dx, origin.y + dy);
					if (!mapData.IsInBounds(target)) continue;
					if (TraceRay(origin, target, mapData, null, out _))
						visible.Add(target);
				}
			}
            
            visible.ExceptWith(ignoredCells ?? new List<Vector2Int>());

			return visible;
		}

		// todo: PassedCells放在TraceRayInfo里
		public bool TraceRay(Vector2Int from, Vector2Int to,  out TraceRayInfo info, List<Vector2Int> passedCells = null) =>
			TraceRay(from, to, _mapService.Data, passedCells, out info);


		// f(t) = from + t * to DDA步进（因为需要检测SceneActor对于视野的影响，所以需要获得视线经过的每个格子，无法单纯枚举网格线）
		// 本质上是在沿射线方向一个个访问其与网格线的交点（沿t从0-1）的方向，核心在于确保顺序
		// 原理上，每轮循环里，计算出和下一个x格线与下一个y格线交点的t值 -> 比较谁的小（先经过谁） -> 步进，迭代
		private static bool TraceRay(Vector2Int from, Vector2Int to, MapData mapData, List<Vector2Int> passedCells, 
			out TraceRayInfo info)
		{
			info = new TraceRayInfo();
			
			if (from == to) return true;

			int dx = to.x - from.x;
			int dy = to.y - from.y;

			if (dx == 0) return MarchAxis(from, to, mapData, false, passedCells, info); // 一条直线，特殊处理
			if (dy == 0) return MarchAxis(from, to, mapData, true, passedCells, info);

			int cellX = from.x; // 目前所在的格子
			int cellY = from.y;
			int stepX = dx > 0 ? 1 : -1;
			int stepY = dy > 0 ? 1 : -1;

			int crossX = Math.Abs(dy);
			int crossY = Math.Abs(dx);
			int stepCrossX = 2 * Math.Abs(dy);
			int stepCrossY = 2 * Math.Abs(dx);

			int maxSteps = Math.Abs(dx) + Math.Abs(dy);

			for (int i = 0; i < maxSteps; i++)
			{
				if (crossX < crossY) // 穿过x格线
				{
					var current = new Vector2Int(cellX, cellY);
					var next = new Vector2Int(cellX + stepX, cellY);
					if (CheckWall(current, next, mapData, info))
						return false;

					cellX += stepX;
					crossX += stepCrossX;
				}
				else if (crossX > crossY) // 穿过y格线
				{
					var current = new Vector2Int(cellX, cellY);
					var next = new Vector2Int(cellX, cellY + stepY);
					if (CheckWall(current, next, mapData, info))
						return false;

					cellY += stepY;
					crossY += stepCrossY;
				}
				else // 穿过网格交点
				{
					var current = new Vector2Int(cellX, cellY);
					var nextX = new Vector2Int(cellX + stepX, cellY);
					var nextY = new Vector2Int(cellX, cellY + stepY);
					var nextXY = new Vector2Int(cellX + stepX, cellY + stepY);
					if (CheckWall(current, nextX, mapData, info) ||
					    CheckWall(current, nextY, mapData, info) ||
					    CheckWall(nextX, nextXY, mapData, info) ||
					    CheckWall(nextY, nextXY, mapData, info))
						return false;

					cellX += stepX;
					cellY += stepY;
					crossX += stepCrossX;
					crossY += stepCrossY;
				}

				var enteredCell = new Vector2Int(cellX, cellY);
				passedCells?.Add(enteredCell);
				info.passedCells.Add(enteredCell);

				if (cellX == to.x && cellY == to.y) // Arrival check
					return true;

				if (IsCellBlocking(new Vector2Int(cellX, cellY), mapData))
					return false;
			}

			return false;
		}

		private static bool MarchAxis(Vector2Int from, Vector2Int to, MapData mapData, bool horizontal, List<Vector2Int> passedCells, TraceRayInfo info)
		{
			int start = horizontal ? from.x : from.y;
			int end = horizontal ? to.x : to.y;
			int step = end > start ? 1 : -1;
			int axis = horizontal ? from.y : from.x;

			int pos = start;
			while (pos != end)
			{
				var current = horizontal ? new Vector2Int(pos, axis) : new Vector2Int(axis, pos);
				var next = horizontal ? new Vector2Int(pos + step, axis) : new Vector2Int(axis, pos + step);

				if (CheckWall(current, next, mapData, info)) return false;

				pos += step;
				var entered = horizontal ? new Vector2Int(pos, axis) : new Vector2Int(axis, pos);
				passedCells?.Add(entered);
				info.passedCells.Add(entered);

				if (pos == end) return true;
				if (IsCellBlocking(entered, mapData)) return false;
			}
			return true;
		}

		private static bool CheckWall(Vector2Int cell1, Vector2Int cell2, MapData mapData, TraceRayInfo info)
		{
			var key = new WallKey(cell1, cell2);
			var wall = mapData.GetWall(key);
			if (wall != null && wall.Type != WallType.None)
			{
				if (wall.Type == WallType.LowWall)
				{
					if (info != null && !info.lowWalls.Contains(key))
					{
						info.lowWalls.Add(key);
					}
					return false; // 低墙不阻挡视线
				}

				if (wall.Type == WallType.HighWall)
				{
					if (info != null && !info.highWalls.Contains(key))
					{
						info.highWalls.Add(key);
					}
				}
				
				return true; // 其他类型的墙阻挡视线
			}
			return false;
		}

		private static bool IsCellBlocking(Vector2Int cellPos, MapData mapData)
		{
			var cell = mapData.GetCell(cellPos);
			return cell?.SceneActor is { BlocksVision: true };
		}
	}

	public class TraceRayInfo
	{
		public List<Vector2Int> passedCells = new();
		public List<WallKey> lowWalls = new();
		public List<WallKey> highWalls = new();
	}
}
