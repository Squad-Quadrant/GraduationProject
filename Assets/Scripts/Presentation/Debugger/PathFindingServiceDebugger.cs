using System.Collections.Generic;
using System.Linq;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.PathFinding;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Pathfinding Service Debugger")]
	public class PathFindingServiceDebugger : MonoBehaviour
	{
		#region Connection

        [TitleGroup("Connection", order: -100)]
        [ShowInInspector, ReadOnly]
        [GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
        private bool IsConnected => _pathfindingService != null;

        [TitleGroup("Connection")]
        [ShowInInspector, ReadOnly, DisplayAsString]
        [HideIf("IsConnected")]
        private string ConnectionHint => "Waiting for Target...";

        #endregion

        #region Test Parameters

        [TitleGroup("Test Parameters", "Configure test pathfinding query")]
        [HorizontalGroup("Test Parameters/Row1")]
        [BoxGroup("Test Parameters/Row1/Origin")]
        [LabelText("Origin"), LabelWidth(50)]
        public Vector2Int testOrigin = new(0, 0);

        [BoxGroup("Test Parameters/Row1/Destination")]
        [LabelText("Target"), LabelWidth(50)]
        public Vector2Int testDestination = new(3, 3);

        [HorizontalGroup("Test Parameters/Row2")]
        [BoxGroup("Test Parameters/Row2/Movement")]
        [LabelText("Max Move Points"), LabelWidth(100)]
        [Range(1, 20)]
        public int testMaxMovement = 5;

        [BoxGroup("Test Parameters/Row2/Options")]
        [LabelText("Pass Allies"), LabelWidth(80)]
        public bool testPassAllies = true;

        [BoxGroup("Test Parameters/Row2/Options")]
        [LabelText("Block Enemies"), LabelWidth(90)]
        public bool testBlockEnemies = true;

        #endregion

        #region Control Panel

        [TitleGroup("Control Panel")]
        [HorizontalGroup("Control Panel/Row1")]
        [Button("Calculate Reachable Area", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1f)]
        [EnableIf("IsConnected")]
        private void CalculateReachableArea()
        {
            var options = new PathFindingOptions(
                canPassThroughAllies: testPassAllies,
                enemiesBlockMovement: testBlockEnemies,
                movingUnitFaction: EUnitFaction.None,
                movingUnitId: null,
                canCrossLowWalls: false,
                canCrossHighWalls: false,
                ignoreTerrainWalkability: false
            );

            _lastReachableResult = _pathfindingService.GetReachableArea(testOrigin, testMaxMovement, options);
            _lastPath = null;
            Debug.Log($"[PathfindingDebugger] {_lastReachableResult}");
        }

        [HorizontalGroup("Control Panel/Row1")]
        [Button("Find Path", ButtonSizes.Large), GUIColor(0.3f, 1f, 0.6f)]
        [EnableIf("IsConnected")]
        private void FindPath()
        {
            var options = new PathFindingOptions(
                canPassThroughAllies: testPassAllies,
                enemiesBlockMovement: testBlockEnemies,
                movingUnitFaction: EUnitFaction.None,
                movingUnitId: null,
                canCrossLowWalls: false,
                canCrossHighWalls: false,
                ignoreTerrainWalkability: false
            );

            _lastPath = _pathfindingService.FindPath(testOrigin, testDestination, options);
            Debug.Log($"[PathfindingDebugger] {_lastPath}");
        }

        [HorizontalGroup("Control Panel/Row2")]
        [Button("Clear Results", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.3f)]
        private void ClearResults()
        {
            _lastReachableResult = null;
            _lastPath = null;
        }

        #endregion

        #region Reachable Area Result

        [TitleGroup("Last Reachable Area", boldTitle: true)]
        [ShowInInspector, ReadOnly]
        [InfoBox("No reachable area calculated", InfoMessageType.None,
            VisibleIf = "@_lastReachableResult == null")]
        [HideIf("@_lastReachableResult == null")]
        private string ReachableAreaSummary => _lastReachableResult?.ToString() ?? "N/A";

        [TitleGroup("Last Reachable Area")]
        [ShowInInspector, ReadOnly]
        [HideIf("@_lastReachableResult == null")]
        [LabelText("Total Reachable")]
        [GUIColor(0.3f, 0.8f, 1f)]
        private int TotalReachable => _lastReachableResult?.ReachableCount ?? 0;

        [TitleGroup("Last Reachable Area")]
        [ShowInInspector, ReadOnly]
        [HideIf("@_lastReachableResult == null")]
        [LabelText("Stoppable Cells")]
        [GUIColor(0.3f, 1f, 0.6f)]
        private int StoppableCount => _lastReachableResult?.StoppableCount ?? 0;

        [TitleGroup("Last Reachable Area")]
        [ShowInInspector, ReadOnly]
        [HideIf("@_lastReachableResult == null")]
        [TableList(AlwaysExpanded = true, IsReadOnly = true, NumberOfItemsPerPage = 10)]
        private List<ReachableCellEntry> ReachableCellList
        {
            get
            {
                if (_lastReachableResult == null)
                    return new List<ReachableCellEntry>();

                return _lastReachableResult.CostMap
                    .OrderBy(kvp => kvp.Value)
                    .Select(kvp => new ReachableCellEntry
                    {
                        position = kvp.Key,
                        cost = kvp.Value,
                        canStop = _lastReachableResult.CanStopAt(kvp.Key)
                    })
                    .ToList();
            }
        }

        #endregion

        #region Path Result

        [TitleGroup("Last Path Result", boldTitle: true)]
        [ShowInInspector, ReadOnly]
        [InfoBox("No path calculated", InfoMessageType.None, VisibleIf = "@_lastPath == null")]
        [HideIf("@_lastPath == null")]
        private string PathSummary => _lastPath?.ToString() ?? "N/A";

        [TitleGroup("Last Path Result")]
        [ShowInInspector, ReadOnly]
        [HideIf("@_lastPath == null || !_lastPath.Found")]
        [LabelText("Path Steps")]
        [GUIColor(0.3f, 1f, 0.6f)]
        private string PathSteps
        {
            get
            {
                if (_lastPath == null || !_lastPath.Found)
                    return "N/A";
                return string.Join(" → ", _lastPath.Path.Select(p => $"({p.x},{p.y})"));
            }
        }

        #endregion

        #region Gizmo Visualization

        [TitleGroup("Visualization")]
        [LabelText("Draw Gizmos")]
        public bool drawGizmos = true;

        [TitleGroup("Visualization")]
        [LabelText("Reachable Color"), ShowIf("drawGizmos")]
        public Color reachableColor = new(0.3f, 0.8f, 1f, 0.5f);

        [TitleGroup("Visualization")]
        [LabelText("Stoppable Color"), ShowIf("drawGizmos")]
        public Color stoppableColor = new(0.3f, 1f, 0.6f, 0.5f);

        [TitleGroup("Visualization")]
        [LabelText("Path Color"), ShowIf("drawGizmos")]
        public Color pathColor = new(1f, 0.8f, 0.3f, 0.8f);

        #endregion

        #region Private Fields

        private IPathFindingService _pathfindingService;
        private IMapService _mapService;
        private ReachableAreaResult _lastReachableResult;
        private PathResult _lastPath;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (Application.isPlaying)
                TryConnect();
        }

        private void Update()
        {
            if (Application.isPlaying && _pathfindingService == null)
                TryConnect();
        }

        private void OnDisable()
        {
            _pathfindingService = null;
            _mapService = null;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || !Application.isPlaying)
                return;

            DrawReachableAreaGizmos();
            DrawPathGizmos();
        }

        #endregion

        #region Connection

        private void TryConnect()
        {
            if (LevelContainer.Instance == null)
                return;

            _pathfindingService = LevelContainer.Instance.TryResolve<IPathFindingService>();
            _mapService = LevelContainer.Instance.TryResolve<IMapService>();
        }

        #endregion

        #region Gizmo Drawing

        private void DrawReachableAreaGizmos()
        {
            if (_lastReachableResult == null || _mapService == null)
                return;

            var converter = LevelContainer.Instance?.TryResolve<ICoordinateConverter>();
            if (converter == null)
                return;

            // Draw all reachable cells
            foreach (var cell in _lastReachableResult.CostMap)
            {
                var worldPos = converter.CellToWorld(cell.Key);
                var color = _lastReachableResult.CanStopAt(cell.Key) ? stoppableColor : reachableColor;

                Gizmos.color = color;
                Gizmos.DrawCube(worldPos, new Vector3(0.8f, 0.8f, 0.1f));
            }
        }

        private void DrawPathGizmos()
        {
            if (_lastPath == null || !_lastPath.Found)
                return;

            var converter = LevelContainer.Instance?.TryResolve<ICoordinateConverter>();
            if (converter == null)
                return;

            Gizmos.color = pathColor;

            for (int i = 0; i < _lastPath.Path.Count - 1; i++)
            {
                var from = converter.CellToWorld(_lastPath.Path[i]);
                var to = converter.CellToWorld(_lastPath.Path[i + 1]);
                Gizmos.DrawLine(from, to);
                Gizmos.DrawSphere(from, 0.15f);
            }

            // Draw destination marker
            if (_lastPath.Path.Count > 0)
            {
                var dest = converter.CellToWorld(_lastPath.Path[^1]);
                Gizmos.DrawSphere(dest, 0.25f);
            }
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class ReachableCellEntry
        {
            [TableColumnWidth(80)]
            public Vector2Int position;

            [TableColumnWidth(50)]
            public int cost;

            [TableColumnWidth(60)]
            [GUIColor("@canStop ? new Color(0.3f, 1f, 0.6f) : new Color(1f, 0.5f, 0.3f)")]
            public bool canStop;
        }

        #endregion
	}
}
