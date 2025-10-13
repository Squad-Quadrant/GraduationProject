using System;
using UnityEngine;
using UnityEditor;
namespace PurpleFlowerCore.Editor.Setting
{
    //todo:设置的保存问题
    public class PFCSettingData : ScriptableObject
    {
        public bool LogInfo = true;
        public bool LogWarning = true;
        public bool LogError = true;
        public bool DebugMode = true;
        public SaveMode SaveMode;
    }

    [CustomEditor(typeof(PFCSettingData))]
    public class PFCSettingDataEditor : UnityEditor.Editor
    {
        private GUIStyle _style;

        private GUIStyle Style
        {
            get
            {
                if(_style == null)
                {
                    _style = new GUIStyle();
                    _style.normal.textColor = Color.white;
                    _style.wordWrap = true;
                }
                return _style;
            }
        }
        
        private void OnEnable()
        {
            PFCSetting.OpenWindow();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("欢迎使用PurpleFlowerCore，这是我为了更好的个性化而制作的程序框架和工具集合\n" +
                                       "该ScriptableObject用于存储PFC设置，请使用菜单栏工具【PFC/打开设置菜单】进行更改",Style, GUILayout.Height(50));
            GUILayout.Space(30);
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
    
    public enum SaveMode
    {
        Json,LitJson,Binary
    }
}