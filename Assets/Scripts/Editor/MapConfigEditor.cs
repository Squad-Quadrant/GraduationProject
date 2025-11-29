#if UNITY_EDITOR
using System.Collections.Generic;
using Data.Config;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public class MapConfigEditor : EditorWindow
	{
		private const string SavePath = "Assets/Data/Map/";
		private readonly List<MapConfig> _mapConfigs = new();

		private MapConfig _currentConfig;
		private MapConfig _dataBuffer;
		private UnityEditor.Editor _currentDataEditor;
        private static MapConfigEditor I { get; set; }

		[MenuItem("Tools/Map Config Editor")]
		private static void OpenWindow()
		{
			var window = GetWindow<MapConfigEditor>();
			window.titleContent = new GUIContent("Map Config Editor");
			window.minSize = new Vector2(400, 500);
			window.Show();
            I = window;
        }
		

		protected void OnEnable()
		{
			FindAllMapConfigs();
		}

		private void OnGUI()
		{
			DrawSelectMap();
			CreateNewMapConfig();
			ShowObject();
			if (_currentConfig != null)
			{
				if (GUILayout.Button("Open Grid Preview"))
				{
                    MapGridPreviewWindow.ShowWindow(_currentConfig);
				}
			}
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
		
		public void CreateNewMapConfig()
		{
			if(!GUILayout.Button("Create New Map Config"))
				return;
			string path = SavePath + "NewMapConfig";
			while (System.IO.File.Exists(path + ".asset"))
			{
				path += "_1";
			}
			path += ".asset";
			if (string.IsNullOrEmpty(path)) return;
			var config = CreateInstance<MapConfig>();
			AssetDatabase.CreateAsset(config, path);
			AssetDatabase.SaveAssets();
			config.Init();
			_currentConfig = config;
			FindAllMapConfigs();
		}

		private void DrawSelectMap()
		{
			EditorGUILayout.LabelField("Select Map Config:", GUILayout.Width(150));
			string[] options = new string[_mapConfigs.Count];
			int currentIndex = -1;
			for (int i = 0; i < _mapConfigs.Count; i++)
			{
				options[i] = _mapConfigs[i].MapName;
				if (_mapConfigs[i] == _currentConfig)
					currentIndex = i;
			}

			int selectedIndex = EditorGUILayout.Popup(currentIndex, options);
			if (selectedIndex != currentIndex && selectedIndex >= 0 && selectedIndex < _mapConfigs.Count)
			{
				_currentConfig = _mapConfigs[selectedIndex];
			}
		}
		
		private void ShowObject()
		{
			if (_currentConfig == null) return;
            
			if (_currentConfig != _dataBuffer)
			{
				_dataBuffer = _currentConfig;
				if (_currentDataEditor != null)
					DestroyImmediate(_currentDataEditor);
				_currentDataEditor = UnityEditor.Editor.CreateEditor(_currentConfig);
			}
			_currentDataEditor.OnInspectorGUI();
		}
        
        public static void HighlightCell(int index)
        {
            // Unity原生和奥丁都无法高亮显示ScriptableObject中的某个字段
            Debug.Log(I._currentConfig.cells[index].position);
        }
        
        public static void HighlightWall(int index)
        {
            Debug.Log(I._currentConfig.walls[index].position1 + " to " + I._currentConfig.walls[index].position2);
        }
	}
}

#endif
