using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.PathFinding.MovementSimulation
{
	public class MovementSimulationResult
	{
		public IReadOnlyList<Vector2Int> ActualPath { get; }

		public bool WasInterrupted { get; } // 是否会被打断

		public IReadOnlyList<Unit.Unit> DiscoveredUnits { get; } // 新发现的Units

		public HashSet<Vector2Int> FinalVisibleCells { get; } // 路径中看到的所有格子

		private MovementSimulationResult(
			IReadOnlyList<Vector2Int> actualPath,
			bool wasInterrupted,
			IReadOnlyList<Unit.Unit> discoveredUnits,
			HashSet<Vector2Int> finalVisibleCells)
		{
			ActualPath = actualPath;
			WasInterrupted = wasInterrupted;
			DiscoveredUnits = discoveredUnits;
			FinalVisibleCells = finalVisibleCells;
		}

		public static MovementSimulationResult Completed(
			IReadOnlyList<Vector2Int> path,
			HashSet<Vector2Int> finalVisibleCells)
		{
			return new MovementSimulationResult(
				path,
				wasInterrupted: false,
				discoveredUnits: Array.Empty<Unit.Unit>(),
				finalVisibleCells);
		}

		public static MovementSimulationResult Interrupted(
			IReadOnlyList<Vector2Int> truncatedPath,
			List<Unit.Unit> discoveredUnits,
			HashSet<Vector2Int> finalVisibleCells)
		{
			return new MovementSimulationResult(
				truncatedPath,
				wasInterrupted: true,
				discoveredUnits,
				finalVisibleCells);
		}
	}
}
