using System;
using UnityEngine;
using PurpleFlowerCore.Base;
using PurpleFlowerCore.PFCDebug;

namespace PurpleFlowerCore
{
    /// <summary>
    /// 在游戏运行时调试系统,在设置中打开"使用运行时Debug菜单"
    /// </summary>
    public static class DebugSystem
    {
        private static bool _isDebugMenuOpen;
        private static DebugMenu _debugMenu;
        private static DebugMenu DebugMenu
        {
            get
            {
                if (_debugMenu is not null) return _debugMenu;
                DebugMenu root = GameObject.Instantiate(Resources.Load<DebugMenu>("PFCRes/DebugMenu"), PFCManager.Canvas.transform);
                root.gameObject.SetActive(false);
                _debugMenu = root;
                return _debugMenu;
            }
        }
#if PFC_DEBUGMENU
        
        // [RuntimeInitializeOnLoadMethod]
        private static void OnGameStart()
        {
            MonoSystem.AddUpdateListener(DebugMenuOpen);
            
        }

        private static void DebugMenuOpen()
        {
            if (!Input.GetKeyDown(KeyCode.BackQuote)) return;
            _isDebugMenuOpen = !_isDebugMenuOpen;
            DebugMenu.gameObject.SetActive(_isDebugMenuOpen);
            DebugMenu.transform.localPosition = Vector3.zero;
            DebugMenu.Switch(_isDebugMenuOpen);
        }
#endif
        /// <summary>
        /// 在Debug菜单中打印一行日志,建议业务中使用PFCLog代替
        /// </summary>
        /// <param name="data"></param>
        public static void Log(LogData data)
        {
            DebugMenu.Log(data);
        }
        
        public static void AddCommand(string commandName, Action command)
        {
#if PFC_DEBUGMENU
            DebugMenu.AddCommand(commandName, command);
#endif
        }

        public static void AddCommand<T>(string commandName, Action<T> command)
        {
#if PFC_DEBUGMENU
            DebugMenu.AddCommand(commandName, command);
#endif
        }

        public static void AddCommand(string commandName, Action<object> command, Type type)
        {
#if PFC_DEBUGMENU
            DebugMenu.AddCommand(commandName, command, type);
#endif
        }


        public static Color GetLogLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => Color.green,
                LogLevel.Info => Color.cyan,
                LogLevel.Warning => Color.yellow,
                LogLevel.Error => Color.red,
                _ => Color.white
            };
        }
    }
}
