#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Data.Config;
using Systems.Map.Config;
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
        private CellConfigData _currentCell;
        private WallConfigData _currentWall;
        private int _currentCellIndex = -1;
        private int _currentWallIndex = -1;
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

        protected void OnDisable()
        {
            ApplyChanges();
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
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            ClearSelect();
            FindConfigFile();
            ShowCurrentCellOrWall();
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
		
		private void CreateNewMapConfig()
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

        private void ClearSelect()
        {
            if (GUILayout.Button("清除选择"))
            {
                _currentCell = null;
                _currentWall = null;
                _currentCellIndex = -1;
                _currentWallIndex = -1;
                Repaint();
            }
        }

        private void FindConfigFile()
        {
            if (GUILayout.Button("定位配置文件"))
            {
                if (_currentConfig != null)
                    EditorGUIUtility.PingObject(_currentConfig);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void ShowCurrentCellOrWall()
        {
            if (_currentConfig == null)
                return;
            
            if (_currentCell == null && _currentWall == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前选中:", EditorStyles.boldLabel);
            
            var so = new SerializedObject(_currentConfig);
            so.Update();

            bool hasChanged = false;

            if (_currentCell != null)
            {
                _currentCellIndex = Array.FindIndex(_currentConfig.cells, c => c != null && c.position == _currentCell.position);
                if (_currentCellIndex >= 0)
                {
                    var prop = so.FindProperty($"cells.Array.data[{_currentCellIndex}]");
                    if (prop != null)
                    {
                        prop.isExpanded = true;
                        
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(prop, new GUIContent("Cell属性"), true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            hasChanged = true;
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("选中的Cell在配置中已不存在。", MessageType.Warning);
                }
            }
            
            if (_currentWall != null)
            {
                _currentWallIndex = Array.FindIndex(_currentConfig.walls, w => w != null && w.Check(_currentWall.position1, _currentWall.position2));
                if (_currentWallIndex >= 0)
                {
                    var prop = so.FindProperty($"walls.Array.data[{_currentWallIndex}]");
                    if (prop != null)
                    {
                        prop.isExpanded = true;
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(prop, new GUIContent("Wall属性"), true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            hasChanged = true;
                        }
                    }
                }
                else
                {
                     EditorGUILayout.HelpBox("选中的Wall在配置中已不存在。", MessageType.Warning);
                }
            }

            if (so.ApplyModifiedProperties() || hasChanged)
            {
                I.Repaint();
            }
        }
        
        private void ApplyChanges()
        {
            EditorUtility.SetDirty(_currentConfig);
            AssetDatabase.SaveAssets();
        }
        
        public static void HighlightCell(int index)
        {
            if (I == null || I._currentConfig == null) return;
            var cells = I._currentConfig.cells;
            if (cells == null || index < 0 || index >= cells.Length) return;

            I._currentCellIndex = index;
            I._currentCell = cells[index];
            I._currentWall = null;
            I._currentWallIndex = -1;
            I.Repaint();
        }

        public static void HighlightWall(int index)
        {
            if (I == null || I._currentConfig == null) return;
            var walls = I._currentConfig.walls;
            if (walls == null || index < 0 || index >= walls.Length) return;

            I._currentWallIndex = index;
            I._currentWall = walls[index];
            I._currentCell = null;
            I._currentCellIndex = -1;
            I.Repaint();
        }
	}
}

#endif
