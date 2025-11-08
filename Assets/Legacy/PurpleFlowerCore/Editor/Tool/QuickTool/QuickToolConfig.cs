using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace PurpleFlowerCore.Editor.Tool
{
    public class QuickToolConfig : ScriptableObject
    {
        public List<QuickToolButtonData> quickToolButtonData = new();

        public static void OpenScene(string scenePath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        }
        
        public static void OpenAsset(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath(path,typeof(Object));
            if(obj == null)
            {
                PFCLog.Error("QuickTool",$"can't find asset:{path}");
                return;
            }
            EditorGUIUtility.PingObject(obj);
            AssetDatabase.OpenAsset(obj);
        }
        
        public static void ShowInExplorer(string path)
        {
            EditorUtility.RevealInFinder(path);
        }
        
        public static void OpenFile(string path)
        {
            EditorUtility.OpenWithDefaultApp(path);
        }
        
        public static void Select(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath(path,typeof(UnityEngine.Object));
            if(obj == null)
            {
                PFCLog.Error("QuickTool",$"can't find asset:{path}");
                return;
            }
            Selection.activeObject = obj;
        }
    }

    [Serializable]
    public class QuickToolButtonData
    {
        public string name;
        public CommandType commandType;
        public Color color;
        public bool lineBreak;
        public string commandParam;
        public UnityEvent command;
        
        public enum CommandType
        {
            OpenScene,
            OpenAsset,
            OpenFile,
            Select,
            ShowInExplorer,
            Custom
        }

        public QuickToolButtonData()
        {
            name = "NewButton";
            commandType = CommandType.OpenScene;
            color = Color.white;
            lineBreak = false;
            commandParam = "";
        }

        public void Command()
        {
            PFCLog.Info("QuickTool",$"execute command:{commandType}({commandParam})");
            switch (commandType)
            {
                case CommandType.OpenScene:
                    QuickToolConfig.OpenScene(commandParam);
                    break;
                case CommandType.OpenAsset:
                    QuickToolConfig.OpenAsset(commandParam);
                    break;
                case CommandType.ShowInExplorer:
                    QuickToolConfig.ShowInExplorer(commandParam);
                    break;
                case CommandType.OpenFile:
                    QuickToolConfig.OpenFile(commandParam);
                    break;
                case CommandType.Select:
                    QuickToolConfig.Select(commandParam);
                    break;
                case CommandType.Custom:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
