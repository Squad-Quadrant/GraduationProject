using System.Collections.Generic;
using Systems.Vision;
using UnityEngine;

namespace Systems.AI
{
	public static class AIVisionHelper
	{
		public static HashSet<Vector2Int> CalculateVisibleCells(
			Unit.Unit unit,
			IVisionCalculator visionCalculator,
			IVisionService visionService)
		{
			if (!unit.CanAIUseEye.Value)
				return new HashSet<Vector2Int> { unit.position };

			return visionCalculator.CalculateVisibleCells(
				unit.position,
				unit.visionRange,
				visionBlockers: visionService.VisionBlockingCells);
		}
	}
}
