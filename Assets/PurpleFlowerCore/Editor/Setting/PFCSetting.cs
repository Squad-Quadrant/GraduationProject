using System;
using System.IO;
using PurpleFlowerCore.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.Setting
{
    public class PFCSetting : EditorWindow
    {
        public PFCSettingData data => SOUtility.GetSOByType<PFCSettingData>(true);
        
        [MenuItem("PFC/打开设置菜单")]
        public static void OpenWindow()
        {
            var win = GetWindow<PFCSetting>("Purple Flower Core");
            win.Show();
            win.maxSize = new Vector2(800, 600);
            win.minSize = win.maxSize;
        }

        public void OnGUI()
        {
            //GUI.DrawTexture(new Rect(600,400,200,200),Resources.Load<Texture>("PFCRes/logo-a0"));
            EditorGUILayout.LabelField("欢迎使用PurpleFlowerCore，这是我为了更好的个性化而制作的程序框架和工具集合\n当前处于早期开发版", GUILayout.Height(50));
            data.SaveMode = (SaveMode)EditorGUILayout.EnumPopup("数据持久化方式", data.SaveMode);
            data.LogInfo = EditorGUILayout.Toggle("使用LogInfo", data.LogInfo);
            data.LogWarning = EditorGUILayout.Toggle("使用LogWarning", data.LogWarning);
            data.LogError = EditorGUILayout.Toggle("使用LogError", data.LogError);
            data.DebugMode = EditorGUILayout.Toggle("使用运行时Debug菜单", data.DebugMode);
            
            if (GUILayout.Button("应用"))
                Apply();
        }
        
        private void Apply()
        {
            ResetMacro();
        }

        private void ResetMacro()
        {
            Array values = Enum.GetValues(typeof(PFCSettingOption));
            foreach (PFCSettingOption value in values)
            {
               PFCEditorUtility.RemoveScriptCompilationSymbol(value);
            }
            
            if(data.SaveMode == SaveMode.Json)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.PFC_JSON);
            }
            else if(data.SaveMode == SaveMode.LitJson)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.PFC_LITJSON);
            }
            else if(data.SaveMode == SaveMode.Binary)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.PFC_BINARY);
            }
            
            if(!data.LogInfo)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.NOT_PFC_LOG_INFO);
            }
            if (!data.LogWarning)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.NOT_PFC_LOG_WARNING);
            }
            if (!data.LogError)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.NOT_PFC_LOG_ERROR);
            }
            if (data.DebugMode)
            {
                PFCEditorUtility.AddScriptCompilationSymbol(PFCSettingOption.PFC_DEBUGMENU);
            }
        }
    }
    
    public enum PFCSettingOption
    {
        PFC_JSON,PFC_LITJSON,PFC_BINARY,
        NOT_PFC_LOG_INFO,NOT_PFC_LOG_WARNING,NOT_PFC_LOG_ERROR,PFC_DEBUGMENU
        
    }
}