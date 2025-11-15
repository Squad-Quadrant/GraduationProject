#if UNITY_EDITOR
using Data.Config;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Systems.Map;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class MapConfigEditor : OdinEditorWindow
	{
		[MenuItem("Tools/Map Config Editor")]
		private static void OpenWindow() => GetWindow<MapConfigEditor>().Show();

		[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
		public MapConfig currentConfig;

		[Button(ButtonSizes.Large), GUIColor(0.3f, 1f, 0.3f)]
		public void CreateNewMapConfig()
		{
			string path = EditorUtility.SaveFilePanelInProject(
				"Create Map Config",
				"NewMapConfig",
				"asset",
				"Choose where to save the map config"
			);

			if (string.IsNullOrEmpty(path)) return;
			var config = CreateInstance<MapConfig>();
			AssetDatabase.CreateAsset(config, path);
			AssetDatabase.SaveAssets();
			currentConfig = config;
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();

			if (currentConfig == null)
				return;

			GUILayout.Space(20);
			SirenixEditorGUI.Title("Grid Preview", "", TextAlignment.Center, true);

			DrawGridPreview();
		}

		private void DrawGridPreview()
		{
			if (currentConfig.cells == null || currentConfig.cells.Length == 0)
			{
				EditorGUILayout.HelpBox("No cells configured. Click 'Generate Default Terrain'.", MessageType.Info);
				return;
			}

			var size = currentConfig.size;
			float cellSize = Mathf.Min(400f / size.x, 400f / size.y);

			Rect gridRect = GUILayoutUtility.GetRect(
				size.x * cellSize,
				size.y * cellSize
			);

			foreach (var cell in currentConfig.cells)
			{
				Rect cellRect = new Rect(
					gridRect.x + cell.position.x * cellSize,
					gridRect.y + (size.y - 1 - cell.position.y) * cellSize,  // 翻转 Y 轴
					cellSize,
					cellSize
				);

				Color color = GetTerrainColor(cell.terrain);
				if (!cell.isWalkable)
					color = Color.gray;

				EditorGUI.DrawRect(cellRect, color);
				GUI.Box(cellRect, GUIContent.none);

				GUIStyle style = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontSize = Mathf.Max(8, (int)(cellSize / 5))
				};
				GUI.Label(cellRect, $"{cell.position.x},{cell.position.y}", style);
			}
		}

		private Color GetTerrainColor(ETerrainType terrain)
		{
			return terrain switch
			{
				_ => Color.white
			};
		}
	}
}

#endif
