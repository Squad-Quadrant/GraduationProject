using System;
using System.Collections.Generic;
using System.Reflection;
using PurpleFlowerCore.Editor.Utility;
using PurpleFlowerCore.Utility;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.Tool
{
    public class DataInspector : EditorWindow
    {
        private Dictionary<MonoBehaviour, List<FieldInfo>> _inspectableFields;
        private Vector2 _scrollPosition;

        [MenuItem("PFC/综合数据检视器")]
        public static void OpenWindow()
        {
            var win = GetWindow<DataInspector>("综合数据检视器");
            win.Show();
        }

        private void OnEnable()
        {
            UpdateInspectableFields();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            UpdateInspectableFields();
            Repaint();
        }

        private void UpdateInspectableFields()
        {
            _inspectableFields = new Dictionary<MonoBehaviour, List<FieldInfo>>();

            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();

            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var type = monoBehaviour.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(InspectableAttribute)))
                    {
                        if (!_inspectableFields.ContainsKey(monoBehaviour))
                        {
                            _inspectableFields[monoBehaviour] = new List<FieldInfo>();
                        }

                        _inspectableFields[monoBehaviour].Add(field);
                    }
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                UpdateInspectableFields();
            }
        }
        
        public void ShowData()
        {
            if (_inspectableFields == null || _inspectableFields.Count == 0)
            {
                EditorGUILayout.LabelField("No inspectable fields found.");
                return;
            }

            foreach (var kvp in _inspectableFields)
            {
                var monoBehaviour = kvp.Key;
                var fields = kvp.Value;

                if (monoBehaviour == null)
                {
                    continue;
                }

                EditorGUILayout.LabelField($"Type: {monoBehaviour.GetType().Name}\tName:{monoBehaviour.name}", EditorStyles.boldLabel);

                foreach (var field in fields)
                {
                    var annotation = (field.GetCustomAttribute(typeof(InspectableAttribute)) as InspectableAttribute)?.Annotation;
                    if (!string.IsNullOrEmpty(annotation))
                    {
                        EditorGUILayout.LabelField(annotation, EditorStyles.miniLabel);
                    }
                    GUI.enabled = false;
                    UIUtility.ShowField(field,monoBehaviour);
                    GUI.enabled = true;
                    EditorGUILayout.Space(1);
                }
                EditorGUILayout.Space();
            }
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            ShowData();
            
            EditorGUILayout.EndScrollView();
        }


    }
}
