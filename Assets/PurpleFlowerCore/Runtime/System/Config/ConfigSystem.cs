using System.Collections.Generic;
using PurpleFlowerCore.Config;
using UnityEngine;

namespace PurpleFlowerCore
{
    public static class ConfigSystem
    {
        private static Dictionary<string, ConfigData> _configData = new Dictionary<string, ConfigData>();
        
        public static void RegisterConfig(ConfigData config)
        {
            var key = config.GetType().Name;
            if (_configData.ContainsKey(key))
            {
                Debug.LogError($"ConfigSystem RegisterConfig Error: {key} already exists");
                return;
            }
            _configData.Add(key, config);
        }
        
        public static T GetConfig<T>() where T : ConfigData
        {
            var key = typeof(T).Name;
            if (_configData.ContainsKey(key))
            {
                return (T)_configData[key];
            }
            Debug.LogError($"ConfigSystem GetConfig Error: {key} not found");
            return null;
        }
        
        public static void LoadAll()
        {
            foreach (var kv in _configData)
            {
                kv.Value.Load();
            }
        }
    }
}