using System.Collections.Generic;
using PurpleFlowerCore.Base;
using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore
{
    public static class UISystem
    {
        private static readonly Dictionary<string, UINode> UIs = new();

        private static Transform _uiRoot;
        
        private static Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    var ui = new GameObject("UI").transform;
                    ui.SetParent(PFCManager.Instance.transform);
                }
                return _uiRoot;
            }
        }
        
        public static bool RegisterUI(string name, UINode ui)
        {
            if (UIs.ContainsKey(name))
            {
                return false;
            }
            UIs.Add(name, ui);
            return true;
        }
        
        public static bool UnRegisterUI(string name)
        {
            if (UIs.ContainsKey(name))
            {
                UIs.Remove(name);
                return true;
            }
            return false;
        }
        
        public static UINode GetUI(string name)
        {
            return UIs.TryGetValue(name, out var ui) ? ui : null;
        }
        
        public static T GetUI<T>(string name) where T : UINode
        {
            return UIs.TryGetValue(name, out var ui) ? ui as T : null;
        }
        
        public static bool ShowUI(string name)
        {
            if (UIs.TryGetValue(name, out var ui))
            {
                ui.Show();
                return true;
            }
            return false;
        }
        
        public static bool HideUI(string name)
        {
            if (UIs.TryGetValue(name, out var ui))
            {
                ui.Hide();
                return true;
            }
            return false;
        }

        public static bool SwitchUI(string name, bool show)
        {
            if (UIs.TryGetValue(name, out var ui))
            {
                ui.Switch(show);
                return true;
            }
            return false;
        }
        
        /// <summary> 
        /// 获取UI层级, 0为最底层
        /// </summary>
        public static Transform GetUILayer(int level)
        {
            
            return null;
        }
    }
}