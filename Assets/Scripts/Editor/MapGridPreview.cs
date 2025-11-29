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

            if (_config.cells == null || _config.cells.Length == 0)
            {
                EditorGUILayout.HelpBox("No cells configured. Click 'Generate Default Terrain' in the editor.",
                    MessageType.Info);
                return;
            }

            var size = _config.Size;
            var cellSize = Mathf.Min(400f / Mathf.Max(1, size.x), 400f / Mathf.Max(1, size.y));
            var totalW = size.x * cellSize;
            var totalH = size.y * cellSize;

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.Height(Mathf.Min(totalH + 10, position.height - 60)));
            var gridRect = GUILayoutUtility.GetRect(totalW, totalH);

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
                GUI.Box(cellRect, GUIContent.none);

                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Max(8, (int)(cellSize / 5))
                };
                GUI.Label(cellRect, $"{cell.position.x},{cell.position.y}", style);
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