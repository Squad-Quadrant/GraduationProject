#if UNITY_EDITOR
using Data.Config;
using Systems.Map;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MapGridPreviewWindow : EditorWindow
    {
        private MapConfig _config;
        private Vector2 _scroll;

        // 固定/可调的单格大小
        private float _cellSize = 32f;
        private const float MinCellSize = 8f;
        private const float MaxCellSize = 128f;

        public static void ShowWindow(MapConfig config)
        {
            var window = GetWindow<MapGridPreviewWindow>("Grid Preview");
            window.minSize = new Vector2(300, 300);
            window._config = config;
            window.Repaint();
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                EditorGUILayout.LabelField("No MapConfig selected.");
                return;
            }

            EditorGUILayout.LabelField($"Preview: {_config.MapName}", EditorStyles.boldLabel);

            // 单格大小控制
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cell Size", GUILayout.Width(70));
            _cellSize = EditorGUILayout.Slider(_cellSize, MinCellSize, MaxCellSize);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                _cellSize = 32f;
            }
            EditorGUILayout.EndHorizontal();

            if (_config.cells == null || _config.cells.Length == 0)
            {
                EditorGUILayout.HelpBox("No cells configured. Click 'Generate Default Terrain' in the editor.",
                    MessageType.Info);
                return;
            }

            var size = _config.Size;
            var cellSize = _cellSize;
            var totalW = size.x * cellSize;
            var totalH = size.y * cellSize;

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.Height(Mathf.Min(totalH + 10, position.height - 80)));

            // 固定大小的 grid 区域
            var gridRect = GUILayoutUtility.GetRect(totalW, totalH, GUILayout.ExpandWidth(false));
            Event e = Event.current;

            // 先绘制所有格子
            foreach (var cell in _config.cells)
            {
                var cellRect = new Rect(
                    gridRect.x + cell.position.x * cellSize,
                    gridRect.y + (size.y - 1 - cell.position.y) * cellSize,
                    cellSize,
                    cellSize
                );

                var color = GetTerrainColor(cell.terrain);
                if (!cell.isWalkable)
                    color = Color.gray;

                EditorGUI.DrawRect(cellRect, color);
                Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Color.black * 0.2f);

                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Max(8, (int)(cellSize / 5))
                };
                GUI.Label(cellRect, $"{cell.position.x},{cell.position.y}", style);

                if (e.type == EventType.MouseDown && e.button == 0 && cellRect.Contains(e.mousePosition))
                {
                    int index = cell.position.y * size.x + cell.position.x;
                    MapConfigEditor.HighlightCell(index);
                    e.Use();
                    Repaint();
                }
            }

            // 绘制墙（在格子间的缝隙位置）
            if (_config.walls != null)
            {
                float thickness = Mathf.Clamp(cellSize * 0.18f, 2f, cellSize * 0.45f);
                for (int i = 0; i < _config.walls.Length; i++)
                {
                    var w = _config.walls[i];
                    if (w == null) continue;
                    var p1 = w.position1;
                    var p2 = w.position2;
                    int dx = p2.x - p1.x;
                    int dy = p2.y - p1.y;

                    Rect wallRect = new Rect();

                    if (Mathf.Abs(dx) == 1 && dy == 0)
                    {
                        // 垂直墙，x 方向相邻
                        int minX = Mathf.Min(p1.x, p2.x);
                        int y = p1.y; // 相同
                        float edgeX = gridRect.x + (minX + 1) * cellSize;
                        float yPos = gridRect.y + (size.y - 1 - y) * cellSize;
                        wallRect = new Rect(edgeX - thickness * 0.5f, yPos, thickness, cellSize);
                    }
                    else if (dx == 0 && Mathf.Abs(dy) == 1)
                    {
                        // 水平墙，y 方向相邻
                        int minY = Mathf.Min(p1.y, p2.y);
                        int x = p1.x; // 相同
                        float edgeY = gridRect.y + (size.y - 1 - minY) * cellSize;
                        float xPos = gridRect.x + x * cellSize;
                        wallRect = new Rect(xPos, edgeY - thickness * 0.5f, cellSize, thickness);
                    }
                    else
                    {
                        // 非相邻的墙（忽略或自定义处理）
                        continue;
                    }

                    EditorGUI.DrawRect(wallRect, new Color(0.15f, 0.15f, 0.15f));
                    // 可选边框
                    Handles.DrawSolidRectangleWithOutline(wallRect, Color.clear, Color.black * 0.25f);

                    if (e.type == EventType.MouseDown && e.button == 0 && wallRect.Contains(e.mousePosition))
                    {
                        MapConfigEditor.HighlightWall(i);
                        e.Use();
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private Color GetTerrainColor(ETerrainType terrain)
        {
            return terrain switch
            {
                ETerrainType.Plain => new Color(0.6f, 1f, 0.6f),
                ETerrainType.Forest => new Color(0.2f, 0.8f, 0.2f),
                ETerrainType.Mountain => new Color(0.5f, 0.5f, 0.5f),
                ETerrainType.Water => new Color(0.2f, 0.4f, 1f),
                _ => Color.white
            };
        }
    }
}
#endif
