#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore
{
    public class DataConfigurator : EditorWindow
    {
        private ScriptableObject _currentData;
        private ScriptableObject _dataBuffer;
        private UnityEditor.Editor _currentDataEditor;
        private List<ScriptableObject> _data;
        public List<ScriptableObject> Data 
        {
            get
            {
                if(_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }

        private GenericMenu _dataTree;

        private const string LogChannel = "综合数据配置";
        private Vector2 _scrollPosition;
        
        
        [MenuItem("PFC/综合数据配置器")]
        public static void OpenWindow()
        {
            var win = GetWindow<DataConfigurator>("综合数据配置器");
            win.Show();
        }
        
        private void OnEnable()
        {
            LoadData();
        }
        
        private void OnValidate()
        {
            LoadData();
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            ShowData();
            EditorGUILayout.EndScrollView();
        }
        
        private void LoadData()
        {
            PFCLog.Info(LogChannel, "LoadData");
            _data = FindConfigurableObjects();
            _dataTree = new GenericMenu();
            foreach (var data in _data)
            {
                var attribute = data.GetType().GetCustomAttribute(typeof(ConfigurableAttribute)) as ConfigurableAttribute;
                if(attribute == null)
                {
                    PFCLog.Error(LogChannel, "数据没有ConfigurableAttribute");
                    continue;
                }

                string path = "";
                if(  attribute.MenuName!=string.Empty) 
                    path = attribute.MenuName[^1] == '/' ? attribute.MenuName : attribute.MenuName + "/";
                path += data.name;
                _dataTree.AddItem(new GUIContent(path), false, () => { _currentData = data; });
            }
        }

        private void ShowData()
        {
            EditorGUILayout.ObjectField("当前数据",_currentData,typeof(ScriptableObject),_currentData);
            var content = new GUIContent("选择数据");
            if (EditorGUILayout.DropdownButton(content,FocusType.Passive))
            {
                _dataTree.DropDown(new Rect(position.width-200,15,10,10));
            }
            ShowObject();
        }
        
        private void ShowObject()
        {
            if (_currentData == null) return;
            
            if (_currentData != _dataBuffer)
            {
                _dataBuffer = _currentData;
                if (_currentDataEditor != null)
                    DestroyImmediate(_currentDataEditor);
                _currentDataEditor = UnityEditor.Editor.CreateEditor(_currentData);
            }
            _currentDataEditor.OnInspectorGUI();
        }
        
        private void Refresh()
        {
            if(!GUILayout.Button("刷新",GUILayout.Height(30),GUILayout.Width(200))) return;
            LoadData();
        }
        
        private void Apply()
        {
            if(!GUILayout.Button("应用",GUILayout.Height(30),GUILayout.Width(200))) return;
        }
        
        private List<ScriptableObject> FindConfigurableObjects()
        {
            var configurableObjects = new List<ScriptableObject>();
            
            var allScriptableObjectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract);
            
            var configurableTypes = allScriptableObjectTypes
                .Where(type => type.GetCustomAttributes(typeof(ConfigurableAttribute), false).Length > 0);

            foreach (var type in configurableTypes)
            {
                var guids = AssetDatabase.FindAssets($"t:{type.Name}");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (obj != null)
                    {
                        configurableObjects.Add(obj);
                    }
                }
            }
            return configurableObjects;
        }
    }
}
#endif
