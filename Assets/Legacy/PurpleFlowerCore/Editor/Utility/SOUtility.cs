using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.Utility
{
    public static class SOUtility
    {
        public const string DefaultSOPath = "Assets/PurpleFlowerCore/Data/";
        public static T GetSOByType<T>(bool createIfNotExist = false) where T : ScriptableObject
        {
            return GetSOByType(typeof(T), createIfNotExist) as T;
        }
        
        public static ScriptableObject GetSOByType(Type type, bool createIfNotExist = false)
        {
            var objs = GetSOsByType(type);
            if(objs.Count>1)
                throw new Exception("There are more than one SOs of type " + type.Name);
            if(objs.Count == 0 && createIfNotExist)
            {
                return CreateSO(type);
            }
            return objs[0];
        }
        
        [Obsolete("Not implemented yet")]
        public static void GetSOByName()
        {
            
        }

        public static List<ScriptableObject> GetSOsByType(Type type)
        {
            if(!type.IsSubclassOf(typeof(ScriptableObject)))
                throw new Exception("Type must be subclass of ScriptableObject");
            
            List<ScriptableObject> res = new();
            var guids = AssetDatabase.FindAssets($"t:{type.Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj != null)
                {
                    res.Add(obj);
                }
            }
            return res;
        }

        public static T CreateSO<T>(string path = "") where T : ScriptableObject
        {
            return CreateSO(typeof(T), path) as T;
        }
        
        public static ScriptableObject CreateSO(Type type, string path = "")
        {
            path = string.IsNullOrEmpty(path)? DefaultSOPath : path;
            var obj = ScriptableObject.CreateInstance(type);
            if(!System.IO.Directory.Exists(path) )
                System.IO.Directory.CreateDirectory(path);
            AssetDatabase.CreateAsset(obj, path + type.Name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return obj;
        }

    }
}