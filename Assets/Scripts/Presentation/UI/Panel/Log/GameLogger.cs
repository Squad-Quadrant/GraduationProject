using System.Collections;
using Core.Log;
using Presentation.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using ILogger = Core.Log.ILogger;

namespace Presentation.UI.Panel.Log
{
    public class GameLogger : UIPanel, ILogger
    {
        [SerializeField] private LogEntry logEntryPrefab;
        [SerializeField] private ScrollRect scrollRect;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            LogExtensions.RegisterGameLogger(this);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            StartCoroutine(ScrollToBottom());
        }

        private void CreateLogEntry(string message, LogLevel level)
        {
            var logEntry = Instantiate(logEntryPrefab, scrollRect.content);
            logEntry.Init(message, level);
            
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ScrollToBottom());
            }
        }
        
        private IEnumerator ScrollToBottom()
        {
            yield return null;
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Debug(string message, bool format = true)
        {
            CreateLogEntry(message, LogLevel.Debug);
        }

        public void Info(string message, bool format = true)
        {
            CreateLogEntry(message, LogLevel.Info);
        }

        public void Warning(string message, bool format = true)
        {
            CreateLogEntry(message, LogLevel.Warning);
        }

        public void Error(string message, bool format = true)
        {
            CreateLogEntry(message, LogLevel.Error);
        }
    }
}