using System.Collections.Generic;
using UnityEngine;

namespace Systems.Vision
{
	public interface IVisionCalculator
	{
		HashSet<Vector2Int> CalculateVisibleCells(Vector2Int origin, int visionRange);

		bool TraceRay(Vector2Int from, Vector2Int to, out TraceRayInfo info, List<Vector2Int> passedCells = null);
	}
}
