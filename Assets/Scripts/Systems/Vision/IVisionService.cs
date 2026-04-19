using System.Collections.Generic;
using UnityEngine;

namespace Systems.Vision
{
    public interface IVisionService
    {
        IReadOnlyCollection<Vector2Int> CurrentVisibleCells { get; }

        bool IsCellVisible(Vector2Int cell);

        // Vision updates
        void RecalculateSharedVision();

        void UpdateUnitVision(string unitId, Vector2Int position, int visionRange);

        void RemoveUnitVision(string unitId);

        // Temporary reveals
        /// <returns>token</returns>
        RevealToken AddTemporaryReveal(IReadOnlyList<Vector2Int> cells);

        void RemoveTemporaryReveal(RevealToken token);

        // Spotted enemies
        IReadOnlyDictionary<string, Vector2Int> SpottedEnemies { get; }

        bool IsEnemySpotted(string unitId);

        Vector2Int? GetSpottedPosition(string unitId);

        void MarkEnemySpotted(string unitId, Vector2Int position);

        void ClearSpottedMark(string unitId);
    }
}