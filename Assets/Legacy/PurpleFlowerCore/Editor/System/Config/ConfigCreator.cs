using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace PurpleFlowerCore
{
    public static class ConfigCreator
    {
        public static void Refresh()
        {
            CreateSO();
        }

        private static void CreateSO()
        {
            var configs = new Dictionary<string, string>();
            
            var allScriptableObjectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableObject)) && !type.IsAbstract);
            Type attributeType = typeof(ConfigurableAttribute);
            var configurableTypes = allScriptableObjectTypes
                .Where(type => type.GetCustomAttributes(attributeType, true).Length > 0);
            foreach (var type in configurableTypes)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets($"t:{type.Name}");
                foreach (var guid in guids)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (obj != null)
                        configs.Add(obj.GetType().Name, path);
                    
                }
            }
        }
    }
}