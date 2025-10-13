using PurpleFlowerCore.Editor.Setting;
using UnityEditor;

namespace PurpleFlowerCore.Editor.Utility
{
    public static class PFCEditorUtility
    {
        /// <summary>
        /// 增加预处理指令
        /// </summary>
        public static void AddScriptCompilationSymbol(string symbolName)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string group = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!group.Contains(symbolName))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, group + ";" + symbolName);
            }
        }
        
        /// <summary>
        /// 移除预处理指令
        /// </summary>
        public static void RemoveScriptCompilationSymbol(string symbolName)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string group = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (group.Contains(symbolName))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, group.Replace(";" + symbolName, string.Empty));
            }
        }
        
        /// <summary>
        /// 增加预处理指令
        /// </summary>
        public static void AddScriptCompilationSymbol(PFCSettingOption symbolName)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string group = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!group.Contains(symbolName.ToString()))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, group + ";" + symbolName);
            }
        }
        
        /// <summary>
        /// 移除预处理指令
        /// </summary>
        public static void RemoveScriptCompilationSymbol(PFCSettingOption symbolName)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string group = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (group.Contains(symbolName.ToString()))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, group.Replace(";" + symbolName, string.Empty));
            }
        }
    }
}