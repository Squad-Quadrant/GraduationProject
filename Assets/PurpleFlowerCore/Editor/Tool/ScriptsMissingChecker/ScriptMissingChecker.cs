using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using GameObjectUtility = UnityEditor.GameObjectUtility;

namespace GP
{
    public class ScriptMissingChecker: EditorWindow
    {
        private readonly List<string> _filter = new();
        private readonly List<GameObject> _gos = new();
        private bool _showGameObject = true;
        private bool _showFilter;
        private string _path = "Assets/";
        private Vector2 _scrollPosition;
        
        [MenuItem("PFC/脚本丢失检查")]
        private static void OpenWindow()
        {
            var win = GetWindow<ScriptMissingChecker>("Script Missing Checker");
            win.Show();
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.LabelField("这是一个检查是否有物体丢失脚本的工具，点击CheckScene检" +
                                     "查场景中的物体，点击CheckAsset检查资源中的物体。可以在" +
                                     "Filter中输入需要忽略的文件夹路径，点击RemoveMissingScripts" +
                                     "将去除GameObjects中的Missing脚本", EditorStyles.wordWrappedLabel);
            _path = EditorGUILayout.TextField("Path", _path);
            _showFilter = EditorGUILayout.Foldout(_showFilter, "Filter");
            if(_showFilter)
            {
                for (int i = 0; i < _filter.Count; i++)
                {
                    _filter[i] = EditorGUILayout.TextField(_filter[i]);
                }
                if (GUILayout.Button("Add Filter"))
                {
                    _filter.Add("");
                }
            }
            _showGameObject = EditorGUILayout.Foldout(_showGameObject, "GameObjects");
            if(_showGameObject)
            {
                foreach (var go in _gos)
                {
                    EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                }
                RemoveMissingScripts();
            }
            CheckAsset();
            CheckScene();
            EditorGUILayout.EndScrollView();
        }
        
        private void CheckScene()
        {
            if (!GUILayout.Button("Check Scene")) return;
            _gos.Clear();
            StringBuilder sb = new StringBuilder();
            GameObject[] gos = FindObjectsOfType<GameObject>();
            foreach (var go in gos)
            {
                int num = 0;
                CheckMissingScript(go, ref num);
                if (num > 0)
                {
                    _gos.Add(go);
                    sb.AppendLine(go.ToString() + $"({num})");
                }
            }
            if(sb.Length == 0)
            {
                Debug.Log("[ScriptMissingChecker] No missing scripts found in Scene.");
                return;
            }
            
            Debug.Log($"[ScriptMissingChecker] Found missing scripts in Scene:\n{sb}");
        }
        
        private void CheckAsset()
        {
            if (!GUILayout.Button("Check Asset")) return;
            _gos.Clear();
            StringBuilder sb = new StringBuilder();
            AssetDatabase.FindAssets("t:GameObject",new []{_path}).ToList().ForEach(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if(_filter.Count > 0 && _filter.Any(filter => !string.IsNullOrEmpty(filter) && path.StartsWith(filter))) return;
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                int num = 0;
                CheckMissingScript(go, ref num);
                if (num > 0)
                {
                    _gos.Add(go);
                    sb.AppendLine(path + $"({num})");
                }
            });
            if(sb.Length == 0)
            {
                Debug.Log("[ScriptMissingChecker] No missing scripts found in Assets.");
                return;
            }
            Debug.Log($"[ScriptMissingChecker] Found missing scripts in Prefabs:\n{sb}");
        }

        private void RemoveMissingScripts()
        {
            if (!GUILayout.Button("Remove Missing Scripts")) return;
            StringBuilder sb = new();
            foreach (var go in _gos)
            { 
                int num = 0;
                DeleteMissingScript(go, ref num);
                if (num > 0)
                {
                    sb.AppendLine(go.ToString() + $"({num})");
                }
            }
            if (sb.Length == 0)
            {
                Debug.Log("[ScriptMissingChecker] No missing scripts found.");
                return;
            }
            Debug.Log($"[ScriptMissingChecker] Removed missing scripts in GameObjects:\n{sb}");
        }
        
        private static void CheckMissingScript(GameObject go, ref int num)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            num += count;
            foreach (Transform child in go.transform)
                CheckMissingScript(child.gameObject,ref num);
        }
        
        private static void DeleteMissingScript(GameObject go, ref int num)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            num += count;
            foreach (Transform child in go.transform)
                DeleteMissingScript(child.gameObject, ref num);
        }
    }
}