#if UNITY_EDITOR
using System.Collections.Generic;
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
		private const string SavePath = "Assets/Data/Map/";
		private readonly List<MapConfig> _mapConfigs = new();

		public MapConfig currentConfig;

		protected override void OnEnable()
		{
			base.OnEnable();
			FindAllMapConfigs();
			minSize = new Vector2(1000, 1000);
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();
			// 下拉列表
			DrawSelectMap();
			CreateNewMapConfig();
			
			GUILayout.Space(20);
			if (currentConfig != null)
			{
				// 显示当前配置的编辑器
				SirenixEditorGUI.Title("Map Config Editor", "", TextAlignment.Center, true);
				
				SirenixEditorGUI.Title("Grid Preview", "", TextAlignment.Center, true);
				DrawGridPreview();
			}
		}
		
		public void CreateNewMapConfig()
		{
			// string path = EditorUtility.SaveFilePanelInProject(
			// 	"Create Map Config",
			// 	"NewMapConfig",
			// 	"asset",
			// 	"Choose where to save the map config"
			// );
			if(!GUILayout.Button("Create New Map Config"))
				return;
			string path = SavePath + "NewMapConfig";
			while (System.IO.File.Exists(path + ".asset"))
			{
				path = path + "_1";
			}

			path += ".asset";

			if (string.IsNullOrEmpty(path)) return;
			var config = CreateInstance<MapConfig>();
			AssetDatabase.CreateAsset(config, path);
			AssetDatabase.SaveAssets();
			config.Init();
			currentConfig = config;
		}

		private void FindAllMapConfigs()
		{
			string[] guids = AssetDatabase.FindAssets("t:MapConfig", new[] { SavePath });
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				MapConfig config = AssetDatabase.LoadAssetAtPath<MapConfig>(assetPath);
				if (config != null)
				{
					_mapConfigs.Add(config);
				}
			}
		}

		private void DrawSelectMap()
		{
			EditorGUILayout.LabelField("Select Map Config:", GUILayout.Width(150));
			string[] options = new string[_mapConfigs.Count];
			int currentIndex = -1;
			for (int i = 0; i < _mapConfigs.Count; i++)
			{
				options[i] = _mapConfigs[i].MapName;
				if (_mapConfigs[i] == currentConfig)
					currentIndex = i;
			}

			int selectedIndex = EditorGUILayout.Popup(currentIndex, options);
			if (selectedIndex != currentIndex && selectedIndex >= 0 && selectedIndex < _mapConfigs.Count)
			{
				currentConfig = _mapConfigs[selectedIndex];
			}
		}
		
		// todo: 当大量图块时会变卡; 点击高亮
		private void DrawGridPreview()
		{
			if (currentConfig.cells == null || currentConfig.cells.Length == 0)
			{
				EditorGUILayout.HelpBox("No cells configured. Click 'Generate Default Terrain'.", MessageType.Info);
				return;
			}

			var size = currentConfig.Size;
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
