using System;
using System.Collections.Generic;
using Core.Log;
using Systems.Map;
using Systems.Map.Config;
using Systems.Unit;
using UnityEngine;

namespace Systems.Vision
{
	public class VisionService : IVisionService
	{
		private readonly IMapService _mapService;
		private readonly IUnitService _unitService;

		public VisionService(IMapService mapService, IUnitService unitService)
		{
			_mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
		}

		// 按曼哈顿距离遍历所有visionRange中的格子，一个个调用HasLineOfSight
		public HashSet<Vector2Int> CalculateVisibleCells(Vector2Int origin, int visionRange)
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

					if (HasLineOfSight(origin, target, mapData))
						visible.Add(target);
				}
			}

			var allUnits = _unitService.GetAllAliveUnits();
			foreach (var unit in allUnits)
			{
				if (unit.faction != EUnitFaction.Player) continue;
				visible.Add(unit.position);
			}

			return visible;
		}

		public bool HasLineOfSight(Vector2Int from, Vector2Int to) => HasLineOfSight(from, to, _mapService.Data);

		// f(t) = from + t * to DDA步进（因为需要检测SceneActor对于视野的影响，所以需要获得视线经过的每个格子，无法单纯枚举网格线）
		// 本质上是在沿射线方向一个个访问其与网格线的交点（沿t从0-1）的方向，核心在于确保顺序
		// 原理上，每轮循环里，计算出和下一个x格线与下一个y格线交点的t值 -> 比较谁的小（先经过谁） -> 步进，迭代
		private bool HasLineOfSight(Vector2Int from, Vector2Int to, MapData mapData)
		{
			if (from == to) return true;

			int dx = to.x - from.x;
			int dy = to.y - from.y;

			if (dx == 0) return MarchAxis(from, to, mapData, false); // 一条直线，特殊处理
			if (dy == 0) return MarchAxis(from, to, mapData, true);

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
					if (IsWallBlocking(current, next, mapData))
						return false;

					cellX += stepX;
					crossX += stepCrossX;
				}
				else if (crossX > crossY) // 穿过y格线
				{
					var current = new Vector2Int(cellX, cellY);
					var next = new Vector2Int(cellX, cellY + stepY);
					if (IsWallBlocking(current, next, mapData))
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
					if (IsWallBlocking(current, nextX, mapData) ||
					    IsWallBlocking(current, nextY, mapData) ||
					    IsWallBlocking(nextX, nextXY, mapData) ||
					    IsWallBlocking(nextY, nextXY, mapData))
						return false;

					cellX += stepX;
					cellY += stepY;
					crossX += stepCrossX;
					crossY += stepCrossY;
				}

				if (cellX == to.x && cellY == to.y) // Arrival check
					return true;

				if (IsCellBlocking(new Vector2Int(cellX, cellY), mapData))
					return false;
			}

			return false;
		}

		private bool MarchAxis(Vector2Int from, Vector2Int to, MapData mapData, bool horizontal)
		{
			int start = horizontal ? from.x : from.y;
			int end = horizontal ? to.x : to.y;
			int step = end > start ? 1 : -1;
			int axis = horizontal ? from.y : from.x;

			int pos = start;
			while (pos != end)
			{
				var current = horizontal
					? new Vector2Int(pos, axis)
					: new Vector2Int(axis, pos);
				var next = horizontal
					? new Vector2Int(pos + step, axis)
					: new Vector2Int(axis, pos + step);

				this.Log($"Checking Wall {current} to {next}: {IsWallBlocking(current, next, mapData)}");
				if (IsWallBlocking(current, next, mapData))
					return false;

				pos += step;

				if (pos == end) return true;

				var entered = horizontal
					? new Vector2Int(pos, axis)
					: new Vector2Int(axis, pos);
				this.Log($"Checking Cell {entered}: {IsCellBlocking(entered, mapData)}");
				if (IsCellBlocking(entered, mapData))
					return false;
			}
			return true;
		}

		private static bool IsWallBlocking(Vector2Int cell1, Vector2Int cell2, MapData mapData)
		{
			var wall = mapData.GetWall(new WallKey(cell1, cell2));
			if (wall == null || wall.WallType == WallType.None) return false;
			return true;
		}

		private static bool IsCellBlocking(Vector2Int cellPos, MapData mapData)
		{
			var cell = mapData.GetCell(cellPos);
			return cell?.SceneActor is { BlocksVision: true };
		}
	}
}
