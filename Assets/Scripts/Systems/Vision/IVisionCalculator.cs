using System.Collections.Generic;
using UnityEngine;

namespace Systems.Vision
{
	public interface IVisionCalculator
	{
		HashSet<Vector2Int> CalculateVisibleCells(Vector2Int origin, int visionRange, List<Vector2Int> ignoredCells = null, IReadOnlyCollection<Vector2Int> visionBlockers = null);

		bool TraceRay(Vector2Int from, Vector2Int to, out TraceRayInfo info, IReadOnlyCollection<Vector2Int> visionBlockers = null);
	}
}
