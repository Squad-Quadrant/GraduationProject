using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.PFCDebug
{
    public class LogPanel : MonoBehaviour
    {
        private const string LogInfoPath = "PFCRes/LogInfo";
        [SerializeField]private ScrollRect scrollRect;
        private HashSet<LogInfo> _logInfos = new();
        [SerializeField] private List<DebugLevelButton> levelButtons = new();
        [SerializeField] private Prompt prompt;
        
        public void Print(LogData data)
        {
            LogInfo logInfo = Instantiate(Resources.Load<LogInfo>(LogInfoPath), scrollRect.content);
            logInfo.Init(data, prompt);
            _logInfos.Add(logInfo);
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }

        public void SwitchLevel(LogLevel level, bool open)
        {
            foreach (var logInfo in _logInfos.Where(logInfo => logInfo.Data.Level == level))
            {
                logInfo.gameObject.SetActive(open);
            }
        }
        
        public void SwitchChannel(string channel, bool open)
        {
            foreach (var logInfo in _logInfos.Where(logInfo => logInfo.Data.Channel == channel))
            {
                logInfo.gameObject.SetActive(open);
            }
        }

        public void Clear()
        {
            foreach (var logInfo in _logInfos)
            {
                Destroy(logInfo.gameObject);
            }
            _logInfos.Clear();
        }

        public void ShowAll()
        {
            foreach (var logInfo in _logInfos)
            {
                logInfo.gameObject.SetActive(true);
            }

            foreach (var button in levelButtons)
            {
                button.SetOpen(true);
            }
        }

        public void Export()
        {
            StringBuilder log = new();
            foreach (var logInfo in _logInfos)
            {
                log.AppendLine(logInfo.Format());
            }
            //todo: 存档系统封装
            if(!Directory.Exists(Application.persistentDataPath + "/log"))
                Directory.CreateDirectory(Application.persistentDataPath + "/log");
            File.WriteAllText(Application.persistentDataPath + "/log/log.txt",log.ToString());
        }
    }
}