#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Systems.Map;
using Systems.Map.Config;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Isometric grid preview window with PNG export.
    ///
    /// Preview: draws an interactive isometric diamond grid in the editor,
    /// showing terrain colors, walls, scene actors, and cell coordinates.
    ///
    /// Export: generates a transparent PNG with only grid lines and an origin
    /// marker, sized to match the game's actual rendering. Art uses this as
    /// a reference layer to draw the ground texture on top of.
    /// </summary>
    public class IsometricGridPreviewWindow : EditorWindow
    {
        private MapConfig _config;
        private Grid _sceneGrid;

        // preview settings
        private float _cellWidth = 80f;
        private bool _showTerrain = true;
        private bool _showWalls = true;
        private bool _showCoords = true;
        private bool _showActors = true;
        private Vector2 _scroll;

        // export settings
        private int _ppu = 400;
        private int _paddingCells = 1;
        private int _exportLineThickness = 3;
        private string _lastExportDir; // Remembers the last export directory within the session

        private float CellHeight => _cellWidth * 0.5f;
        private float HalfW => _cellWidth * 0.5f;
        private float HalfH => CellHeight * 0.5f;

        private float _lastWorldMinX;
        private float _lastWorldMinY;
        private bool _hasAtlasOrigin;

        [MenuItem("Tools/Isometric Grid Preview")]
        private static void OpenFromMenu()
        {
            var window = GetWindow<IsometricGridPreviewWindow>("Isometric Preview");
            window.minSize = new Vector2(500, 400);
        }

        /// <summary>
        /// Open the window with a specific MapConfig pre-loaded.
        /// Called from MapConfigEditor.
        /// </summary>
        public static void ShowWindow(MapConfig config)
        {
            var window = GetWindow<IsometricGridPreviewWindow>("Isometric Preview");
            window.minSize = new Vector2(500, 400);
            window._config = config;
            window.Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!_config || _config.cells == null || _config.cells.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Select a MapConfig with initialized cells to preview.",
                    MessageType.Info);
                return;
            }

            DrawIsometricPreview();
        }

        private void DrawToolbar()
        {
            // Row 1: Config selection + display toggles
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Map", GUILayout.Width(30));
            _config = (MapConfig)EditorGUILayout.ObjectField(
                _config, typeof(MapConfig), false, GUILayout.Width(200));

            GUILayout.Space(10);
            _showTerrain = GUILayout.Toggle(_showTerrain, "Terrain", EditorStyles.toolbarButton);
            _showWalls = GUILayout.Toggle(_showWalls, "Walls", EditorStyles.toolbarButton);
            _showCoords = GUILayout.Toggle(_showCoords, "Coords", EditorStyles.toolbarButton);
            _showActors = GUILayout.Toggle(_showActors, "Actors", EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            // Row 2: Cell size slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cell Width", GUILayout.Width(65));
            _cellWidth = EditorGUILayout.Slider(_cellWidth, 30f, 200f);
            if (GUILayout.Button("Reset", GUILayout.Width(50))) _cellWidth = 80f;
            EditorGUILayout.EndHorizontal();

            // Row 3: Export controls
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Grid", GUILayout.Width(30));
            _sceneGrid = (Grid)EditorGUILayout.ObjectField(
                _sceneGrid, typeof(Grid), true, GUILayout.Width(200));

            GUILayout.Space(5);
            EditorGUILayout.LabelField("PPU", GUILayout.Width(28));
            _ppu = EditorGUILayout.IntField(_ppu, GUILayout.Width(50));

            EditorGUILayout.LabelField("Pad", GUILayout.Width(25));
            _paddingCells = EditorGUILayout.IntField(_paddingCells, GUILayout.Width(30));

            EditorGUILayout.LabelField("Line", GUILayout.Width(28));
            _exportLineThickness = EditorGUILayout.IntSlider(_exportLineThickness, 1, 5, GUILayout.Width(120));

            GUILayout.Space(5);
            GUI.enabled = _sceneGrid;
            if (GUILayout.Button("Export PNG", GUILayout.Width(80)))
                ExportPNG();
            GUI.enabled = true;

            if (!_sceneGrid)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("(Assign scene Grid to enable export)", EditorStyles.miniLabel, GUILayout.Width(200));
            }

	        GUI.enabled = _hasAtlasOrigin;
	        if (GUILayout.Button("Copy Origin", GUILayout.Width(80)))
	        {
		        GUIUtility.systemCopyBuffer = $"{_lastWorldMinX:F6},{_lastWorldMinY:F6}";
		        Debug.Log($"[IsometricGridPreview] Copied to clipboard: {_lastWorldMinX:F6},{_lastWorldMinY:F6}");
	        }
	        GUI.enabled = true;

	        GUILayout.FlexibleSpace();
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.Space(2);
        }

        #region Isometric Preview Drawing

        /// <summary>
        /// Draws the isometric diamond grid in a scrollable area.
        ///
        /// Coordinate system (GUI space, Y-down):
        ///   - basisX = (cellWidth/2, cellHeight/2)  → right-down
        ///   - basisY = (-cellWidth/2, cellHeight/2)  → left-down
        ///   - Cell (0,0) is at the bottom of the diamond grid.
        ///   - Cell center for (x,y) = origin + ((x-y)*halfW, -(x+y)*halfH)
        /// </summary>
        private void DrawIsometricPreview()
        {
            var mapSize = _config.Size;
            float hw = HalfW;
            float hh = HalfH;

            // Compute the bounding box of the entire diamond grid in local space.
            // The grid spans from cell (0,0) at the bottom to cell (W-1, H-1) at the top.
            // We need to find the extremes of all diamond vertices.
            ComputeGridBounds(mapSize, hw, hh,
                out float minX, out float minY, out float maxX, out float maxY);

            float contentW = maxX - minX;
            float contentH = maxY - minY;
            float margin = 20f;

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.Height(Mathf.Min(contentH + margin * 2 + 10, position.height - 110)));

            // Reserve space in the layout for the grid
            var layoutRect = GUILayoutUtility.GetRect(
                contentW + margin * 2,
                contentH + margin * 2,
                GUILayout.ExpandWidth(false));

            // Origin: offset so that the top-left of the bounding box sits at (margin, margin)
            var origin = new Vector2(
                layoutRect.x + margin - minX,
                layoutRect.y + margin - minY);

            // Draw layers bottom-up: terrain fill → grid lines → walls → actors → coords
            if (_showTerrain) DrawTerrainFill(mapSize, origin);
            DrawGridLines(mapSize, origin);
            if (_showWalls) DrawWalls(origin);
            if (_showActors) DrawActors(mapSize, origin);
            if (_showCoords) DrawCoordinates(mapSize, origin);

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Compute axis-aligned bounding box of the diamond grid in local preview space.
        /// Each cell's diamond has 4 vertices offset from its center by ±(hw, 0) and ±(0, hh).
        /// </summary>
        private void ComputeGridBounds(Vector2Int mapSize, float hw, float hh,
            out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = float.MaxValue;
            minY = float.MaxValue;
            maxX = float.MinValue;
            maxY = float.MinValue;

            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    var center = CellToLocal(x, y, hw, hh);
                    // Diamond extremes
                    minX = Mathf.Min(minX, center.x - hw);
                    maxX = Mathf.Max(maxX, center.x + hw);
                    minY = Mathf.Min(minY, center.y - hh);
                    maxY = Mathf.Max(maxY, center.y + hh);
                }
            }
        }

        /// <summary>
        /// Fill each cell's diamond with terrain-based color.
        /// Uses Handles.DrawSolidRectangleWithOutline which accepts arbitrary quad vertices.
        /// </summary>
        private void DrawTerrainFill(Vector2Int mapSize, Vector2 origin)
        {
            foreach (var cell in _config.cells)
            {
                var center = CellToScreen(cell.position.x, cell.position.y, origin);
                var verts = GetDiamondVerts3D(center);
                var color = GetTerrainColor(cell.Terrain);
                color.a = 0.5f;
                Handles.DrawSolidRectangleWithOutline(verts, color, Color.clear);
            }
        }

        /// <summary>
        /// Draw diamond outlines for all cells.
        /// </summary>
        private void DrawGridLines(Vector2Int mapSize, Vector2 origin)
        {
            var lineColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            foreach (var cell in _config.cells)
            {
                var center = CellToScreen(cell.position.x, cell.position.y, origin);
                var v = GetDiamondVerts2D(center);

                Handles.color = lineColor;
                Handles.DrawLine(V3(v[0]), V3(v[1])); // Top → Right
                Handles.DrawLine(V3(v[1]), V3(v[2])); // Right → Bottom
                Handles.DrawLine(V3(v[2]), V3(v[3])); // Bottom → Left
                Handles.DrawLine(V3(v[3]), V3(v[0])); // Left → Top
            }
        }

        /// <summary>
        /// Draw walls as thick colored lines on the shared diamond edges.
        ///
        /// Edge mapping (with Y-flipped preview, (0,0) at bottom):
        ///   Wall between (x,y) and (x+1,y):
        ///     shared edge = cell(x,y).Right → cell(x,y).Top
        ///   Wall between (x,y) and (x,y+1):
        ///     shared edge = cell(x,y).Left → cell(x,y).Top
        /// </summary>
        private void DrawWalls(Vector2 origin)
        {
            if (_config.walls == null) return;

            foreach (var wall in _config.walls)
            {
                if (wall == null || wall.WallType == WallType.None) continue;

                var p1 = wall.position1;
                var p2 = wall.position2;
                int dx = p2.x - p1.x;
                int dy = p2.y - p1.y;

                // Use position1 as the reference cell
                var center = CellToScreen(p1.x, p1.y, origin);
                float hw = HalfW, hh = HalfH;

                Vector2 edgeStart, edgeEnd;

                if (dx == 1 && dy == 0)
                {
                    // Wall between (x,y) and (x+1,y) → Right→Top edge
                    edgeStart = center + new Vector2(hw, 0);
                    edgeEnd = center + new Vector2(0, -hh);
                }
                else if (dx == -1 && dy == 0)
                {
                    // Reversed: use p2 as reference
                    center = CellToScreen(p2.x, p2.y, origin);
                    edgeStart = center + new Vector2(hw, 0);
                    edgeEnd = center + new Vector2(0, -hh);
                }
                else if (dx == 0 && dy == 1)
                {
                    // Wall between (x,y) and (x,y+1) → Left→Top edge
                    edgeStart = center + new Vector2(-hw, 0);
                    edgeEnd = center + new Vector2(0, -hh);
                }
                else if (dx == 0 && dy == -1)
                {
                    // Reversed: use p2 as reference
                    center = CellToScreen(p2.x, p2.y, origin);
                    edgeStart = center + new Vector2(-hw, 0);
                    edgeEnd = center + new Vector2(0, -hh);
                }
                else continue; // Non-adjacent wall, skip

                var wallColor = GetWallColor(wall.WallType);
                Handles.color = wallColor;
                Handles.DrawAAPolyLine(4f, V3(edgeStart), V3(edgeEnd));
            }
        }

        /// <summary>
        /// Draw scene actor markers as a cross (×) in the center of occupied cells.
        /// </summary>
        private void DrawActors(Vector2Int mapSize, Vector2 origin)
        {
            var actorColor = new Color(1f, 1f, 0f, 0.9f);
            float markSize = HalfH * 0.4f;

            foreach (var cell in _config.cells)
            {
                if (cell.sceneActor == null) continue;

                var center = CellToScreen(cell.position.x, cell.position.y, origin);

                Handles.color = actorColor;
                Handles.DrawAAPolyLine(3f,
                    V3(center + new Vector2(-markSize, -markSize)),
                    V3(center + new Vector2(markSize, markSize)));
                Handles.DrawAAPolyLine(3f,
                    V3(center + new Vector2(markSize, -markSize)),
                    V3(center + new Vector2(-markSize, markSize)));

                // Draw extra cells for multi-cell actors
                if (cell.sceneActor.ExtraGrid == null) continue;
                foreach (var offset in cell.sceneActor.ExtraGrid)
                {
                    int ex = cell.position.x + offset.x;
                    int ey = cell.position.y + offset.y;
                    if (ex < 0 || ex >= mapSize.x || ey < 0 || ey >= mapSize.y) continue;

                    var extraCenter = CellToScreen(ex, ey, origin);
                    Handles.color = actorColor * 0.7f;
                    Handles.DrawAAPolyLine(2f,
                        V3(extraCenter + new Vector2(-markSize, -markSize)),
                        V3(extraCenter + new Vector2(markSize, markSize)));
                    Handles.DrawAAPolyLine(2f,
                        V3(extraCenter + new Vector2(markSize, -markSize)),
                        V3(extraCenter + new Vector2(-markSize, markSize)));
                }
            }
        }

        /// <summary>
        /// Draw coordinate labels at each cell center.
        /// </summary>
        private void DrawCoordinates(Vector2Int mapSize, Vector2 origin)
        {
            int fontSize = Mathf.Clamp((int)(_cellWidth / 6f), 7, 14);
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                normal = { textColor = new Color(1f, 1f, 1f, 0.85f) }
            };

            float labelW = _cellWidth * 0.6f;
            float labelH = fontSize + 4;

            foreach (var cell in _config.cells)
            {
                var center = CellToScreen(cell.position.x, cell.position.y, origin);
                var labelRect = new Rect(
                    center.x - labelW * 0.5f,
                    center.y - labelH * 0.5f,
                    labelW, labelH);
                GUI.Label(labelRect, $"{cell.position.x},{cell.position.y}", style);
            }
        }

        #endregion

        #region PNG Export

        private void ExportPNG()
        {
            if (!_sceneGrid)
            {
                EditorUtility.DisplayDialog("Export Failed",
                    "Assign a scene Grid component to export.", "OK");
                return;
            }

            if (!_config)
            {
                EditorUtility.DisplayDialog("Export Failed",
                    "No MapConfig selected.", "OK");
                return;
            }

            var center00 = (Vector2)_sceneGrid.GetCellCenterWorld(Vector3Int.zero);
            var center10 = (Vector2)_sceneGrid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
            var center01 = (Vector2)_sceneGrid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
            Vector2 basisX = center10 - center00; // World offset per +1 in grid X
            Vector2 basisY = center01 - center00; // World offset per +1 in grid Y

            var mapSize = _config.Size;
            int padCells = Mathf.Max(0, _paddingCells);

            // --- Step 2: Compute world-space bounding box ---
            float worldMinX = float.MaxValue, worldMinY = float.MaxValue;
            float worldMaxX = float.MinValue, worldMaxY = float.MinValue;

            // Effective range with padding
            int startCell = -padCells;
            int endX = mapSize.x + padCells;
            int endY = mapSize.y + padCells;

            for (int gy = startCell; gy < endY; gy++)
            {
                for (int gx = startCell; gx < endX; gx++)
                {
                    Vector2 cellCenter = center00 + gx * basisX + gy * basisY;

                    // Diamond vertices in world space (half basis offsets)
                    Vector2 top   = cellCenter + 0.5f * basisX + 0.5f * basisY;
                    Vector2 right = cellCenter + 0.5f * basisX - 0.5f * basisY;
                    Vector2 bot   = cellCenter - 0.5f * basisX - 0.5f * basisY;
                    Vector2 left  = cellCenter - 0.5f * basisX + 0.5f * basisY;

                    ExpandBounds(top, ref worldMinX, ref worldMinY, ref worldMaxX, ref worldMaxY);
                    ExpandBounds(right, ref worldMinX, ref worldMinY, ref worldMaxX, ref worldMaxY);
                    ExpandBounds(bot, ref worldMinX, ref worldMinY, ref worldMaxX, ref worldMaxY);
                    ExpandBounds(left, ref worldMinX, ref worldMinY, ref worldMaxX, ref worldMaxY);
                }
            }

            // --- Step 3: Pixel dimensions ---
            int imgW = Mathf.CeilToInt((worldMaxX - worldMinX) * _ppu) + 2;
            int imgH = Mathf.CeilToInt((worldMaxY - worldMinY) * _ppu) + 2;

            if (imgW > 8192 || imgH > 8192)
            {
                if (!EditorUtility.DisplayDialog("Large Image Warning",
                    $"The export will be {imgW}×{imgH} pixels. Continue?",
                    "Continue", "Cancel"))
                    return;
            }

            // --- Step 4: Create texture and draw layers ---
            var tex = new Texture2D(imgW, imgH, TextureFormat.RGBA32, false);
            var clearPixels = new Color[imgW * imgH]; // All transparent
            tex.SetPixels(clearPixels);

            int thickness = Mathf.Max(1, _exportLineThickness);

            // Build a lookup for cell configs by position (for walkability and actors)
            var cellLookup = new Dictionary<Vector2Int, CellConfigData>();
            if (_config.cells != null)
            {
                foreach (var cell in _config.cells)
                    cellLookup[cell.position] = cell;
            }

            // Layer 1: Cell walkability fill
            // Walkable cells get a subtle green tint; non-walkable cells get a red tint.
            // This lets art see at a glance which areas are passable.
            var walkableColor = new Color(0.2f, 0.8f, 0.2f, 0.15f);
            var blockedColor  = new Color(1.0f, 0.2f, 0.2f, 0.25f);

            for (int gy = 0; gy < mapSize.y; gy++)
            {
                for (int gx = 0; gx < mapSize.x; gx++)
                {
                    Vector2 cellCenter = center00 + gx * basisX + gy * basisY;
                    var pos = new Vector2Int(gx, gy);

                    Vector2 top   = cellCenter + 0.5f * basisX + 0.5f * basisY;
                    Vector2 right = cellCenter + 0.5f * basisX - 0.5f * basisY;
                    Vector2 bot   = cellCenter - 0.5f * basisX - 0.5f * basisY;
                    Vector2 left  = cellCenter - 0.5f * basisX + 0.5f * basisY;

                    var pTop   = WorldToPixel(top, worldMinX, worldMinY, imgH);
                    var pRight = WorldToPixel(right, worldMinX, worldMinY, imgH);
                    var pBot   = WorldToPixel(bot, worldMinX, worldMinY, imgH);
                    var pLeft  = WorldToPixel(left, worldMinX, worldMinY, imgH);

                    bool walkable = !cellLookup.TryGetValue(pos, out var cellData) || cellData.IsWalkable;
                    FillDiamond(tex, pTop, pRight, pBot, pLeft, walkable ? walkableColor : blockedColor);
                }
            }

            // Layer 2: Grid lines (white outlines for all cells)
            var lineColor = Color.white;

            for (int gy = 0; gy < mapSize.y; gy++)
            {
                for (int gx = 0; gx < mapSize.x; gx++)
                {
                    Vector2 cellCenter = center00 + gx * basisX + gy * basisY;

                    Vector2 top   = cellCenter + 0.5f * basisX + 0.5f * basisY;
                    Vector2 right = cellCenter + 0.5f * basisX - 0.5f * basisY;
                    Vector2 bot   = cellCenter - 0.5f * basisX - 0.5f * basisY;
                    Vector2 left  = cellCenter - 0.5f * basisX + 0.5f * basisY;

                    var pTop   = WorldToPixel(top, worldMinX, worldMinY, imgH);
                    var pRight = WorldToPixel(right, worldMinX, worldMinY, imgH);
                    var pBot   = WorldToPixel(bot, worldMinX, worldMinY, imgH);
                    var pLeft  = WorldToPixel(left, worldMinX, worldMinY, imgH);

                    DrawThickLine(tex, pTop, pRight, lineColor, thickness);
                    DrawThickLine(tex, pRight, pBot, lineColor, thickness);
                    DrawThickLine(tex, pBot, pLeft, lineColor, thickness);
                    DrawThickLine(tex, pLeft, pTop, lineColor, thickness);
                }
            }

            // Layer 3: Walls (colored lines on shared diamond edges)
            if (_config.walls != null)
            {
                int wallThickness = thickness + 2; // Slightly thicker than grid lines

                foreach (var wall in _config.walls)
                {
                    if (wall == null || wall.WallType == WallType.None) continue;

                    var p1 = wall.position1;
                    var p2 = wall.position2;
                    int dx = p2.x - p1.x;
                    int dy = p2.y - p1.y;

                    // Normalize direction: always use the cell with the smaller coordinate as ref
                    int refX, refY;
                    if (dx == 1 && dy == 0) { refX = p1.x; refY = p1.y; }
                    else if (dx == -1 && dy == 0) { refX = p2.x; refY = p2.y; dx = 1; }
                    else if (dx == 0 && dy == 1) { refX = p1.x; refY = p1.y; }
                    else if (dx == 0 && dy == -1) { refX = p2.x; refY = p2.y; dy = 1; }
                    else continue;

                    Vector2 refCenter = center00 + refX * basisX + refY * basisY;

                    Vector2 edgeStart, edgeEnd;
                    if (dx == 1)
                    {
                        // Wall between (x,y) and (x+1,y) → Right→Top edge
                        edgeStart = refCenter + 0.5f * basisX - 0.5f * basisY; // Right vertex
                        edgeEnd   = refCenter + 0.5f * basisX + 0.5f * basisY; // Top vertex
                    }
                    else
                    {
                        // Wall between (x,y) and (x,y+1) → Left→Top edge
                        edgeStart = refCenter - 0.5f * basisX + 0.5f * basisY; // Left vertex
                        edgeEnd   = refCenter + 0.5f * basisX + 0.5f * basisY; // Top vertex
                    }

                    var wColor = GetWallColor(wall.WallType);
                    var pStart = WorldToPixel(edgeStart, worldMinX, worldMinY, imgH);
                    var pEnd   = WorldToPixel(edgeEnd, worldMinX, worldMinY, imgH);
                    DrawThickLine(tex, pStart, pEnd, wColor, wallThickness);
                }
            }

            // Layer 4: Scene actor markers (filled dot at cell center)
            var actorColor = new Color(1f, 1f, 0f, 0.9f);
            int actorDotRadius = Mathf.Max(4, thickness * 3);

            foreach (var cell in _config.cells!)
            {
                if (!cell.sceneActor) continue;

                Vector2 cellCenter = center00 + cell.position.x * basisX + cell.position.y * basisY;
                var px = WorldToPixel(cellCenter, worldMinX, worldMinY, imgH);
                DrawDot(tex, px, actorColor, actorDotRadius);

                // Extra cells for multi-cell actors
                if (cell.sceneActor.ExtraGrid == null) continue;
                foreach (var offset in cell.sceneActor.ExtraGrid)
                {
                    int ex = cell.position.x + offset.x;
                    int ey = cell.position.y + offset.y;
                    if (ex < 0 || ex >= mapSize.x || ey < 0 || ey >= mapSize.y) continue;

                    Vector2 extraCenter = center00 + ex * basisX + ey * basisY;
                    var epx = WorldToPixel(extraCenter, worldMinX, worldMinY, imgH);
                    DrawDot(tex, epx, actorColor * 0.7f, actorDotRadius);
                }
            }

            // Layer 5: Origin marker — red dot at cell (0,0) for orientation
            var originPx = WorldToPixel(center00, worldMinX, worldMinY, imgH);
            int originDotRadius = Mathf.Max(5, thickness * 3);
            DrawDot(tex, originPx, Color.red, originDotRadius);

            // --- Step 6: Save via file dialog ---
            tex.Apply();
            byte[] pngData = tex.EncodeToPNG();
            DestroyImmediate(tex);

            string defaultName = $"{_config.MapName}_grid_{imgW}x{imgH}";
            string defaultDir = _lastExportDir ?? "Assets/Art/MapReference";

            string fullPath = EditorUtility.SaveFilePanel(
                "Export Grid Reference PNG",
                defaultDir,
                defaultName,
                "png");

            if (string.IsNullOrEmpty(fullPath)) return; // User cancelled

            _lastExportDir = Path.GetDirectoryName(fullPath);

            File.WriteAllBytes(fullPath, pngData);

            // Refresh AssetDatabase only if saved inside the project
            if (fullPath.StartsWith(Application.dataPath))
                AssetDatabase.Refresh();

            Debug.Log($"[IsometricGridPreview] Exported {imgW}×{imgH} grid to: {fullPath}");
            EditorUtility.RevealInFinder(fullPath);
            Debug.Log($"[IsometricGridPreview] Atlas origin: worldMin=({worldMinX:F6}, {worldMinY:F6}), PPU={_ppu}");
	        _lastWorldMinX = worldMinX;
	        _lastWorldMinY = worldMinY;
	        _hasAtlasOrigin = true;
        }

        #endregion

        #region Coordinate Helpers

        /// <summary>
        /// Convert grid coordinates to local preview space (relative to origin = 0,0).
        /// GUI coordinate system: X right, Y down.
        ///
        /// For isometric layout:
        ///   screenX = (x - y) * halfWidth
        ///   screenY = -(x + y) * halfHeight
        /// Y is negated so that cell (0,0) sits at the bottom of the grid,
        /// matching the in-game coordinate system.
        /// </summary>
        private static Vector2 CellToLocal(int x, int y, float hw, float hh)
        {
            return new Vector2((x - y) * hw, -(x + y) * hh);
        }

        /// <summary>
        /// CellToLocal + offset to screen position.
        /// </summary>
        private Vector2 CellToScreen(int x, int y, Vector2 origin)
        {
            return origin + CellToLocal(x, y, HalfW, HalfH);
        }

        /// <summary>
        /// Get the 4 diamond vertices in 2D screen space (GUI coordinates).
        /// Order: Top, Right, Bottom, Left (clockwise from top).
        /// </summary>
        private Vector2[] GetDiamondVerts2D(Vector2 center)
        {
            float hw = HalfW, hh = HalfH;
            return new[]
            {
                center + new Vector2(0, -hh),  // Top
                center + new Vector2(hw, 0),   // Right
                center + new Vector2(0, hh),   // Bottom
                center + new Vector2(-hw, 0),  // Left
            };
        }

        /// <summary>
        /// Get diamond vertices as Vector3 for Handles.DrawSolidRectangleWithOutline.
        /// </summary>
        private Vector3[] GetDiamondVerts3D(Vector2 center)
        {
            var v = GetDiamondVerts2D(center);
            return new[] { V3(v[0]), V3(v[1]), V3(v[2]), V3(v[3]) };
        }

        /// <summary>
        /// Convert a world-space position to pixel coordinates in the export texture.
        /// Texture origin is bottom-left, so Y is flipped.
        /// </summary>
        private Vector2Int WorldToPixel(Vector2 worldPos, float minX, float minY, int imgH)
        {
            int px = Mathf.RoundToInt((worldPos.x - minX) * _ppu) + 1;
            int py = Mathf.RoundToInt((worldPos.y - minY) * _ppu) + 1;
            // No Y flip needed: Texture2D (0,0) is bottom-left, matching world Y-up
            return new Vector2Int(px, py);
        }

        private static void ExpandBounds(Vector2 p,
            ref float minX, ref float minY, ref float maxX, ref float maxY)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        /// <summary>Vector2 → Vector3 for Handles API.</summary>
        private static Vector3 V3(Vector2 v) => new(v.x, v.y, 0f);

        #endregion

        #region Texture Drawing Helpers

        /// <summary>
        /// Bresenham's line algorithm. Draws a 1px line between two points on a Texture2D.
        /// </summary>
        private static void DrawLine(Texture2D tex, Vector2Int from, Vector2Int to, Color color)
        {
            int x0 = from.x, y0 = from.y;
            int x1 = to.x, y1 = to.y;
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int w = tex.width, h = tex.height;

            while (true)
            {
                if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
                    tex.SetPixel(x0, y0, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx)  { err += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// Draw a line with thickness by offsetting perpendicular to the line direction.
        /// </summary>
        private static void DrawThickLine(Texture2D tex, Vector2Int from, Vector2Int to,
            Color color, int thickness)
        {
            if (thickness <= 1)
            {
                DrawLine(tex, from, to, color);
                return;
            }

            float dx = to.x - from.x;
            float dy = to.y - from.y;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) return;

            // Unit normal perpendicular to the line
            float nx = -dy / len;
            float ny = dx / len;

            int half = thickness / 2;
            for (int t = -half; t <= half; t++)
            {
                int ox = Mathf.RoundToInt(nx * t);
                int oy = Mathf.RoundToInt(ny * t);
                DrawLine(tex,
                    new Vector2Int(from.x + ox, from.y + oy),
                    new Vector2Int(to.x + ox, to.y + oy),
                    color);
            }
        }

        /// <summary>
        /// Draw a filled circle on the texture (used for origin marker).
        /// </summary>
        private static void DrawDot(Texture2D tex, Vector2Int center, Color color, int radius)
        {
            int w = tex.width, h = tex.height;
            int r2 = radius * radius;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy > r2) continue;
                    int px = center.x + dx;
                    int py = center.y + dy;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                        tex.SetPixel(px, py, color);
                }
            }
        }

        /// <summary>
        /// Fill a diamond (quadrilateral) defined by four vertices using scanline.
        /// Vertices are: top, right, bottom, left (in pixel coordinates).
        /// Uses alpha blending so fills can layer without overwriting.
        /// </summary>
        private static void FillDiamond(Texture2D tex,
            Vector2Int top, Vector2Int right, Vector2Int bot, Vector2Int left, Color color)
        {
            int w = tex.width, h = tex.height;

            // Find Y range (texture Y, bottom-up)
            int minY = Mathf.Min(Mathf.Min(top.y, right.y), Mathf.Min(bot.y, left.y));
            int maxY = Mathf.Max(Mathf.Max(top.y, right.y), Mathf.Max(bot.y, left.y));
            minY = Mathf.Max(0, minY);
            maxY = Mathf.Min(h - 1, maxY);

            // The diamond has two halves split at the horizontal midline.
            // Bottom half: from bot.y to left.y/right.y (edges: bot→left and bot→right)
            // Top half: from left.y/right.y to top.y (edges: left→top and right→top)
            // Since left.y == right.y for a symmetric diamond, the midY is at that level.
            int midY = left.y; // == right.y for standard isometric

            for (int py = minY; py <= maxY; py++)
            {
                float xLeft, xRight;

                if (py <= midY)
                {
                    // Bottom half: edges are bot→left and bot→right
                    float t = (midY == bot.y) ? 0f : (float)(py - bot.y) / (midY - bot.y);
                    xLeft  = Mathf.Lerp(bot.x, left.x, t);
                    xRight = Mathf.Lerp(bot.x, right.x, t);
                }
                else
                {
                    // Top half: edges are left→top and right→top
                    float t = (top.y == midY) ? 0f : (float)(py - midY) / (top.y - midY);
                    xLeft  = Mathf.Lerp(left.x, top.x, t);
                    xRight = Mathf.Lerp(right.x, top.x, t);
                }

                int startX = Mathf.Max(0, Mathf.CeilToInt(Mathf.Min(xLeft, xRight)));
                int endX   = Mathf.Min(w - 1, Mathf.FloorToInt(Mathf.Max(xLeft, xRight)));

                for (int px = startX; px <= endX; px++)
                {
                    // Alpha blend over existing pixel
                    Color existing = tex.GetPixel(px, py);
                    Color blended = Color.Lerp(existing, color, color.a);
                    blended.a = Mathf.Max(existing.a, color.a);
                    tex.SetPixel(px, py, blended);
                }
            }
        }

        #endregion

        #region Color Helpers

        /// <summary>
        /// Reuse the same terrain color scheme as MapGridPreviewWindow.
        /// </summary>
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
                WallType.LowWall  => new Color(1f, 0.6f, 0.2f),
                WallType.HighWall => new Color(1f, 0.2f, 0.2f),
                _                 => Color.gray
            };
        }

        #endregion
    }
}
#endif
