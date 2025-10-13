using System;
using PurpleFlowerCore.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.Tool
{
    public sealed class EditorQuickTools : EditorWindow
    {
        private QuickToolConfig _config;
        private QuickToolConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = SOUtility.GetSOByType<QuickToolConfig>(true);
                }
                return _config;
            }
        }

        private bool _openConfigPanel;

        private Vector2 _scrollPosition;
        
        [MenuItem("PFC/快速跳转工具", false, 302)]
        public static void CreateWindow()
        {
            var window = GetWindow<EditorQuickTools>();
            window.titleContent = new GUIContent("Quick Tool");
            window.minSize = new Vector2(150f, 50f);
            window.Show();
        }

        private void OnGUI()
        {
            try
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                foreach (var button in Config.quickToolButtonData)
                {
                    if (button.lineBreak)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    GUI.backgroundColor = button.color;
                    if (GUILayout.Button(button.name))
                    {
                        try
                        {
                            if (button.commandType == QuickToolButtonData.CommandType.Custom)
                                button.command?.Invoke();
                            else
                                button.Command();
                        }
                        catch (Exception e)
                        {
                            PFCLog.Error("QuickTool", e.Message);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();
                
                _openConfigPanel = EditorGUILayout.Foldout(_openConfigPanel, new GUIContent("配置面板"));
                if (_openConfigPanel)
                {
                    ConfigPanel();
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }            
            catch (Exception e)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                var style = new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = Color.red}};
                EditorGUILayout.LabelField(e.Message,style);
                throw;
            }
        }

        private void ConfigPanel()
        {
            if (GUILayout.Button("Add Button"))
            {
                Config.quickToolButtonData.Add(new QuickToolButtonData());
            }
            
            if (GUILayout.Button("Save"))
            {
                SaveConfig();
            }

            try
            {
                foreach (var buttonConfig in Config.quickToolButtonData)
                {
                    EditorGUILayout.Space(10);
                    buttonConfig.name = EditorGUILayout.TextField("Name", buttonConfig.name);
                    buttonConfig.lineBreak = EditorGUILayout.Toggle("Line Break", buttonConfig.lineBreak);
                    buttonConfig.color = EditorGUILayout.ColorField("Color", buttonConfig.color);
                    buttonConfig.commandType =
                        (QuickToolButtonData.CommandType)EditorGUILayout.EnumPopup("Command Type",
                            buttonConfig.commandType);
                    if (buttonConfig.commandType == QuickToolButtonData.CommandType.Custom)
                    {
                        EditorGUILayout.LabelField("not implemented", EditorStyles.boldLabel);
                    }
                    else
                    {
                        buttonConfig.commandParam =
                            EditorGUILayout.TextField("Command Param", buttonConfig.commandParam);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        Config.quickToolButtonData.Remove(buttonConfig);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is not InvalidOperationException)
                    throw;
            }
        }
        
        private void SaveConfig()
        {
            EditorUtility.SetDirty(Config);
            AssetDatabase.SaveAssets();
            PFCLog.Info("QuickTool", "Config Saved");
        }
    }
}
