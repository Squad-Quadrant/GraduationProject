using Core.Log;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Log
{
    public class LogEntry : MonoBehaviour
    {
        [SerializeField] private Text logText;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private float padding = 5;

        public void Init(string message, LogLevel level)
        {
            logText.text = message;
            logText.color = GetLogLevelColor(level);
            layoutElement.preferredHeight = logText.preferredHeight + padding;
            // todo: 变成最下层的条目
            
        }
        
        private static Color GetLogLevelColor(LogLevel level)
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