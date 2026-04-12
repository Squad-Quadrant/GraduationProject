using System.Collections.Generic;
using UnityEngine;

namespace Systems.Vision
{
	public interface IVisionService
	{
		IReadOnlyCollection<Vector2Int> CurrentVisibleCells { get; }

		bool IsCellVisible(Vector2Int cell);

		// Vision updates
		void UpdateVisionForUnit(Unit.Unit unit);

		void UpdateVisionAtPosition(Vector2Int position, int visionRange, string unitId);

		void UpdateVisionByPrecomputed(HashSet<Vector2Int> cells, string unitId); // 防止移动模拟卡住

		// Temporary reveals
		/// <returns>token</returns>
		int AddTemporaryReveal(IReadOnlyList<Vector2Int> cells);

		void RemoveTemporaryReveal(int token);

		// Spotted enemies
		IReadOnlyDictionary<string, Vector2Int> SpottedEnemies { get; }

		bool IsEnemySpotted(string unitId);

		Vector2Int? GetSpottedPosition(string unitId);

		void MarkEnemySpotted(string unitId, Vector2Int position);

		void ClearSpottedMark(string unitId);
	}
}
