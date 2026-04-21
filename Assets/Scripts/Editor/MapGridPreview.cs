#if UNITY_EDITOR
using Systems.Map;
using Systems.Map.Config;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MapGridPreviewWindow : EditorWindow
    {
        private MapConfig _config;
        private Vector2 _scroll;

        private float _cellSlotSize = 40f;
        private float _cellGap = 10f;
        private const float MinSlotSize = 8f;
        private const float MaxSlotSize = 256f;
        private const float MinGap = 0f;
        private const float MaxGap = 64f;

        private bool _showTerrain = true;
        private bool _showWalls = true;
        private bool _showActors = true;
        private bool _showRegions;
        private bool _showCoords = true;

        private GUIStyle _coordLabelStyle;

        private float Slot => _cellSlotSize;
        private float Gap => Mathf.Clamp(_cellGap, 0f, _cellSlotSize * 0.8f);
        private float Inset => Gap * 0.5f;
        private float CellDrawSize => Mathf.Max(0f, Slot - Gap);

        public static void ShowWindow(MapConfig config)
        {
            var window = GetWindow<MapGridPreviewWindow>("Grid Preview");
            window.minSize = new Vector2(300, 300);
            window._config = config;
            window.Repaint();
        }

        private void OnEnable() => RebuildStyles();

        private void OnGUI()
        {
            if (!_config)
            {
                EditorGUILayout.LabelField("No MapConfig selected.");
                return;
            }

            DrawToolbar();

            if (_config.cells == null || _config.cells.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No cells configured. Click 'Generate Default Terrain' in the editor.",
                    MessageType.Info);
                return;
            }

            DrawGrid();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.LabelField($"Preview: {_config.MapName}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slot Size", GUILayout.Width(60));
            _cellSlotSize = EditorGUILayout.Slider(_cellSlotSize, MinSlotSize, MaxSlotSize);
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
                _cellSlotSize = 40f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gap", GUILayout.Width(60));
            _cellGap = EditorGUILayout.Slider(_cellGap, MinGap, Mathf.Min(MaxGap, _cellSlotSize * 0.8f));
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
                _cellGap = 4f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _showTerrain = GUILayout.Toggle(_showTerrain, "Terrain", EditorStyles.toolbarButton);
            _showRegions = GUILayout.Toggle(_showRegions, "Regions", EditorStyles.toolbarButton);
            _showWalls   = GUILayout.Toggle(_showWalls,   "Walls",   EditorStyles.toolbarButton);
            _showActors  = GUILayout.Toggle(_showActors,  "Actors",  EditorStyles.toolbarButton);
            _showCoords  = GUILayout.Toggle(_showCoords,  "Coords",  EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            var size = _config.Size;
            float slot = Slot;
            float totalW = size.x * slot;
            float totalH = size.y * slot;

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.Height(Mathf.Min(totalH + 10, position.height - 120)));

            var gridRect = GUILayoutUtility.GetRect(totalW, totalH, GUILayout.ExpandWidth(false));
            if (_coordLabelStyle == null) RebuildStyles();
            if (_showTerrain) DrawCells(gridRect, size);
            if (_showRegions) DrawRegionOverlay(gridRect, size);
            if (_showWalls)   DrawWalls(gridRect, size);
            if (_showActors)  DrawActorOverlays(gridRect, size);
            if (_showCoords)  DrawCoordLabels(gridRect, size);
            HandleCellClicks(gridRect, size);
            HandleWallClicks(gridRect, size);

            EditorGUILayout.EndScrollView();
        }

        private void DrawCells(Rect gridRect, Vector2Int size)
        {
            float slot = Slot, inset = Inset, cellSize = CellDrawSize;

            foreach (var cell in _config.cells)
            {
                var rect = GetCellRect(gridRect, cell.position, size.y, slot, inset, cellSize);
                var color = cell.IsWalkable
                    ? GetTerrainColor(cell.Terrain)
                    : new Color(0.25f, 0.25f, 0.25f); // Dark gray for non-walkable
                EditorGUI.DrawRect(rect, color);
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.black * 0.2f);
            }
        }

        private void DrawRegionOverlay(Rect gridRect, Vector2Int size)
        {
            float slot = Slot, inset = Inset, cellSize = CellDrawSize;

            foreach (var cell in _config.cells)
            {
                // Region 0 (default outdoor) is not colored to avoid visual noise
                if (cell.regionId == 0) continue;

                var rect = GetCellRect(gridRect, cell.position, size.y, slot, inset, cellSize);
                var color = GetRegionColor(cell.regionId);
                EditorGUI.DrawRect(rect, color);
            }
        }

        private static Color GetRegionColor(int regionId)
        {
            // Golden angle ≈ 137.5° gives maximum hue separation
            float hue = (regionId * 0.618034f) % 1f;
            var color = Color.HSVToRGB(hue, 0.6f, 0.9f);
            color.a = 0.35f;
            return color;
        }

        private void DrawCoordLabels(Rect gridRect, Vector2Int size)
        {
            float slot = Slot, inset = Inset, cellSize = CellDrawSize;
            _coordLabelStyle.fontSize = Mathf.Max(8, (int)(cellSize / 5));

            foreach (var cell in _config.cells)
            {
                var rect = GetCellRect(gridRect, cell.position, size.y, slot, inset, cellSize);

                // When region overlay is active, append regionId for quick verification
                string label = _showRegions && cell.regionId != 0
                    ? $"{cell.position.x},{cell.position.y} [{cell.regionId}]"
                    : $"{cell.position.x},{cell.position.y}";

                GUI.Label(rect, label, _coordLabelStyle);
            }
        }
        private void DrawWalls(Rect gridRect, Vector2Int size)
        {
            if (_config.walls == null) return;

            float slot = Slot, gap = Gap, inset = Inset, cellSize = CellDrawSize;
            float thickness = Mathf.Max(1f, gap);

            foreach (var w in _config.walls)
            {
                if (w == null || w.WallType == WallType.None) continue;

                var wallRect = ComputeWallRect(gridRect, size, w, slot, inset, cellSize, thickness);
                if (!wallRect.HasValue) continue;

                EditorGUI.DrawRect(wallRect.Value, GetWallColor(w.WallType));
                Handles.DrawSolidRectangleWithOutline(wallRect.Value, Color.clear, Color.black * 0.25f);
            }
        }

        private static Rect? ComputeWallRect(
            Rect gridRect, Vector2Int size, WallConfigData w,
            float slot, float inset, float cellSize, float thickness)
        {
            int dx = w.position2.x - w.position1.x;
            int dy = w.position2.y - w.position1.y;

            if (Mathf.Abs(dx) == 1 && dy == 0)
            {
                // Vertical wall (between x-adjacent cells)
                int minX = Mathf.Min(w.position1.x, w.position2.x);
                int y = w.position1.y;
                float edgeX = gridRect.x + (minX + 1) * slot;
                float slotY = gridRect.y + (size.y - 1 - y) * slot;
                return new Rect(edgeX - thickness * 0.5f, slotY + inset, thickness, cellSize);
            }

            if (dx == 0 && Mathf.Abs(dy) == 1)
            {
                // Horizontal wall (between y-adjacent cells)
                int minY = Mathf.Min(w.position1.y, w.position2.y);
                int x = w.position1.x;
                float edgeY = gridRect.y + (size.y - 1 - minY) * slot;
                float slotX = gridRect.x + x * slot;
                return new Rect(slotX + inset, edgeY - thickness * 0.5f, cellSize, thickness);
            }

            return null; // Non-adjacent wall, skip
        }

        private void DrawActorOverlays(Rect gridRect, Vector2Int size)
        {
            float slot = Slot, inset = Inset, cellSize = CellDrawSize;
            var actorColor = new Color(1f, 1f, 0f);

            foreach (var cell in _config.cells)
            {
                if (cell.sceneActor == null) continue;

                // Draw cross on base cell
                var baseRect = GetCellRect(gridRect, cell.position, size.y, slot, inset, cellSize);
                DrawActorCross(baseRect, cellSize, actorColor);

                // Draw cross on extra cells
                foreach (var offset in cell.sceneActor.extraGrid)
                {
                    var extraPos = cell.position + offset;
                    if (extraPos.x < 0 || extraPos.x >= size.x ||
                        extraPos.y < 0 || extraPos.y >= size.y)
                        continue;

                    var extraRect = GetCellRect(gridRect, extraPos, size.y, slot, inset, cellSize);
                    DrawActorCross(extraRect, cellSize, actorColor);
                }
            }
        }

        private static void DrawActorCross(Rect cellRect, float cellSize, Color color)
        {
            float frac = 0.3f;
            float w = cellSize * frac;
            float h = cellSize * frac;

            // Vertical bar
            var vRect = new Rect(
                cellRect.x + (cellRect.width - w) * 0.5f,
                cellRect.y, w, cellRect.height);
            EditorGUI.DrawRect(vRect, color);
            Handles.DrawSolidRectangleWithOutline(vRect, Color.clear, Color.black * 0.25f);

            // Horizontal bar
            var hRect = new Rect(
                cellRect.x,
                cellRect.y + (cellRect.height - h) * 0.5f,
                cellRect.width, h);
            EditorGUI.DrawRect(hRect, color);
            Handles.DrawSolidRectangleWithOutline(hRect, Color.clear, Color.black * 0.25f);
        }

        private void HandleCellClicks(Rect gridRect, Vector2Int size)
        {
            Event e = Event.current;
            if (e.type != EventType.MouseDown || e.button != 0) return;

            float slot = Slot, inset = Inset, cellSize = CellDrawSize;

            foreach (var cell in _config.cells)
            {
                var rect = GetCellRect(gridRect, cell.position, size.y, slot, inset, cellSize);
                if (!rect.Contains(e.mousePosition)) continue;

                int index = cell.position.y * size.x + cell.position.x;
                MapConfigEditor.HighlightCell(index);
                e.Use();
                Repaint();
                return;
            }
        }

        private void HandleWallClicks(Rect gridRect, Vector2Int size)
        {
            if (_config.walls == null) return;

            Event e = Event.current;
            if (e.type != EventType.MouseDown || e.button != 0) return;

            float slot = Slot, gap = Gap, inset = Inset, cellSize = CellDrawSize;
            float thickness = Mathf.Max(1f, gap);

            for (int i = 0; i < _config.walls.Length; i++)
            {
                var w = _config.walls[i];
                if (w == null) continue;

                var wallRect = ComputeWallRect(gridRect, size, w, slot, inset, cellSize, thickness);
                if (!wallRect.HasValue || !wallRect.Value.Contains(e.mousePosition)) continue;

                MapConfigEditor.HighlightWall(i);
                e.Use();
                Repaint();
                return;
            }
        }

        private static Rect GetCellRect(
            Rect gridRect, Vector2Int pos, int mapHeight,
            float slot, float inset, float cellSize)
        {
            float slotX = gridRect.x + pos.x * slot;
            float slotY = gridRect.y + (mapHeight - 1 - pos.y) * slot;
            return new Rect(slotX + inset, slotY + inset, cellSize, cellSize);
        }

        private void RebuildStyles()
        {
            _coordLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.black }
            };
        }

        private static Color GetTerrainColor(ETerrainType terrain)
        {
            return terrain switch
            {
                ETerrainType.Plain    => new Color(0.6f, 1f, 0.6f),
                ETerrainType.Forest   => new Color(0.2f, 0.8f, 0.2f),
                ETerrainType.Mountain => new Color(0.5f, 0.5f, 0.5f),
                ETerrainType.Water    => new Color(0.2f, 0.4f, 1f),
                _                     => Color.white
            };
        }

        private static Color GetWallColor(WallType wallType)
        {
            return wallType switch
            {
                WallType.LowWall  => new Color(0.5f, 0.3f, 0.1f),
                WallType.HighWall  => new Color(1f, 0.2f, 0.2f),
                _                  => Color.gray
            };
        }
    }
}
#endif
