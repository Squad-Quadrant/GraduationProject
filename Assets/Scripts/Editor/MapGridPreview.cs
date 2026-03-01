#if UNITY_EDITOR
using System.Collections.Generic;
using Data.Config;
using Data.Config.Map;
using Systems.Map;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MapGridPreviewWindow : EditorWindow
    {
        private MapConfig _config;
        private Vector2 _scroll;

        // 固定/可调的单格槽位大小（包含缝隙）和缝隙大小
        private float _cellSlotSize = 40f; // 一个格子占据的槽位大小（包括缝隙）
        private float _cellGap = 10f;       // 格子之间的缝隙（像素）
        private const float MinSlotSize = 8f;
        private const float MaxSlotSize = 256f;
        private const float MinGap = 0f;
        private const float MaxGap = 64f;

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

            // 控制面板：槽位大小 & 缝隙
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slot Size", GUILayout.Width(70));
            _cellSlotSize = EditorGUILayout.Slider(_cellSlotSize, MinSlotSize, MaxSlotSize);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
                _cellSlotSize = 40f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gap", GUILayout.Width(70));
            _cellGap = EditorGUILayout.Slider(_cellGap, MinGap, Mathf.Min(MaxGap, _cellSlotSize * 0.8f));
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
                _cellGap = 4f;
            EditorGUILayout.EndHorizontal();

            if (_config.cells == null || _config.cells.Length == 0)
            {
                EditorGUILayout.HelpBox("No cells configured. Click 'Generate Default Terrain' in the editor.",
                    MessageType.Info);
                return;
            }

            var size = _config.Size;
            var slot = _cellSlotSize;
            var gap = Mathf.Clamp(_cellGap, 0f, slot * 0.8f);
            var totalW = size.x * slot;
            var totalH = size.y * slot;

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.Height(Mathf.Min(totalH + 10, position.height - 100)));

            var gridRect = GUILayoutUtility.GetRect(totalW, totalH, GUILayout.ExpandWidth(false));
            Event e = Event.current;

            // 绘制格子（使用槽位位置，但实际绘制区域向内缩进 gap/2）
            float inset = gap * 0.5f;
            float cellDrawSize = Mathf.Max(0f, slot - gap);
            var actorOverlays = new List<(Rect rect, Color color)>();

            foreach (var cell in _config.cells)
            {
                var slotX = gridRect.x + cell.position.x * slot;
                var slotY = gridRect.y + (size.y - 1 - cell.position.y) * slot;

                var cellRect = new Rect(
                    slotX + inset,
                    slotY + inset,
                    cellDrawSize,
                    cellDrawSize
                );

                var color = GetTerrainColor(cell.Terrain);

                EditorGUI.DrawRect(cellRect, color);
                Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Color.black * 0.2f);

                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Max(8, (int)(cellDrawSize / 5))
                };
                GUI.Label(cellRect, $"{cell.position.x},{cell.position.y}", style);

                // 绘制 SceneActor 的像素标识
                if (cell.sceneActor != null)
                {
                    Color actorColor = GetActorColor();
                    float frac = 0.3f; // 比例
                    float w = cellDrawSize * frac;
                    float h = cellDrawSize * frac;

                    // 纵向中心条带（存入列表）
                    var vRect = new Rect(cellRect.x + (cellRect.width - w) * 0.5f, cellRect.y, w, cellRect.height);
                    actorOverlays.Add((vRect, actorColor));
                    // 横向中心条带（存入列表）
                    var hRect = new Rect(cellRect.x, cellRect.y + (cellRect.height - h) * 0.5f, cellRect.width, h);
                    actorOverlays.Add((hRect, actorColor));

                    // ExtraGrid: 只计算并加入列表，不立即绘制
                    foreach (var extra in cell.sceneActor.ExtraGrid)
                    {
                        int extraX = cell.position.x + extra.x;
                        int extraY = cell.position.y + extra.y;
                        if (extraX < 0 || extraX >= size.x || extraY < 0 || extraY >= size.y)
                            continue;

                        var extraSlotX = gridRect.x + extraX * slot;
                        var extraSlotY = gridRect.y + (size.y - 1 - extraY) * slot;
                        var extraCellRect = new Rect(
                            extraSlotX + inset,
                            extraSlotY + inset,
                            cellDrawSize,
                            cellDrawSize );

                        var evRect = new Rect(extraCellRect.x + (extraCellRect.width - w) * 0.5f, extraCellRect.y, w, extraCellRect.height);
                        actorOverlays.Add((evRect, actorColor));
                        var ehRect = new Rect(extraCellRect.x, extraCellRect.y + (extraCellRect.height - h) * 0.5f, extraCellRect.width, h);
                        actorOverlays.Add((ehRect, actorColor));
                    }
                }

                if (e.type == EventType.MouseDown && e.button == 0 && cellRect.Contains(e.mousePosition))
                {
                    int index = cell.position.y * size.x + cell.position.x;
                    MapConfigEditor.HighlightCell(index);
                    e.Use();
                    Repaint();
                }
            }

            // 绘制墙：在槽位边界的缝隙区域内绘制，不与 cellRect 重叠
            if (_config.walls != null)
            {
                float thickness = Mathf.Max(1f, gap); // 墙的厚度使用 gap（或最小值 1）
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
                        // 垂直墙（x 相邻），位于两个槽位的垂直边界
                        int minX = Mathf.Min(p1.x, p2.x);
                        int y = p1.y; // 相同 y
                        float edgeX = gridRect.x + (minX + 1) * slot;
                        float slotY = gridRect.y + (size.y - 1 - y) * slot;
                        wallRect = new Rect(
                            edgeX - thickness * 0.5f,
                            slotY + inset,
                            thickness,
                            cellDrawSize
                        );
                    }
                    else if (dx == 0 && Mathf.Abs(dy) == 1)
                    {
                        // 水平墙（y 相邻），位于两个槽位的水平边界
                        int minY = Mathf.Min(p1.y, p2.y);
                        int x = p1.x; // 相同 x
                        float edgeY = gridRect.y + (size.y - 1 - minY) * slot; // boundary line between minY and minY+1
                        float slotX = gridRect.x + x * slot;
                        wallRect = new Rect(
                            slotX + inset,
                            edgeY - thickness * 0.5f,
                            cellDrawSize,
                            thickness
                        );
                    }
                    else
                    {
                        // 非相邻或斜墙：忽略或自定义处理
                        continue;
                    }

                    Color wallColor = GetWallColor(w.WallType);
                    EditorGUI.DrawRect(wallRect, wallColor);
                    Handles.DrawSolidRectangleWithOutline(wallRect, Color.clear, Color.black * 0.25f);

                    if (e.type == EventType.MouseDown && e.button == 0 && wallRect.Contains(e.mousePosition))
                    {
                        MapConfigEditor.HighlightWall(i);
                        e.Use();
                        Repaint();
                    }
                }
            }
            
            foreach (var (rect, color) in actorOverlays)
            {
                EditorGUI.DrawRect(rect, color);
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.black * 0.25f);
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

        private Color GetWallColor(WallType wallType)
        {
            return wallType switch
            {
                WallType.LowWall => new Color(0.5f, 0.3f, 0.1f),
                WallType.HighWall => new Color(1f, 0.2f, 0.2f),
                _ => Color.gray
            };
        }

        private Color GetActorColor()
        {
            return new Color(1f, 1f, 0f);
        }
    }
}
#endif
