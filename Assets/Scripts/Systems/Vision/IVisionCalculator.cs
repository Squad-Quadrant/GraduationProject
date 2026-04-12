using System.Collections.Generic;
using UnityEngine;

namespace Systems.Vision
{
	public interface IVisionCalculator
	{
		HashSet<Vector2Int> CalculateVisibleCells(Vector2Int origin, int visionRange);

		bool HasLineOfSight(Vector2Int from, Vector2Int to);
	}
}
