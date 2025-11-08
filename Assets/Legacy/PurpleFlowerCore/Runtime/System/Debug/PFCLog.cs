using System.Diagnostics;
using UnityEngine;

namespace PurpleFlowerCore
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }
    public struct LogData
    {
        public LogLevel Level;
        public string Channel;
        public string Content;
        public string Time;
        public Color Color;
        public StackFrame[] StackFrames;
    }
    
    public static class PFCLog
    {
        public static void Print(LogLevel level,Color color, string channel, object content)
        {
            var prefixColor = DebugSystem.GetLogLevelColor(level);
            var prefix = level.ToString();
            string fullContent;
            if (channel == null)
            {
                fullContent = $"<color=#{ColorUtility.ToHtmlStringRGB(prefixColor)}>[{prefix}]</color>" +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{content}</color>";
            }
            else
            {
                fullContent = $"<color=#{ColorUtility.ToHtmlStringRGB(prefixColor)}>[{prefix}][{channel}]</color>" +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{content}</color>";
            }
            switch(level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(fullContent);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(fullContent);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(fullContent);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(fullContent);
                    break;
                default:
                    UnityEngine.Debug.Log(fullContent);
                    break;
            }
#if PFC_DEBUGMENU
            // if(Application.isPlaying)
            // {
            //     StackTrace stackTrace = new StackTrace();
            //     StackFrame[] stackFrames = stackTrace.GetFrames()?[2..];
            //     var logData = new LogData
            //     {
            //         Level = level,
            //         Channel = channel,
            //         Content = content.ToString(),
            //         Time = System.DateTime.Now.ToString("HH:mm:ss"),
            //         Color = color,
            //         StackFrames = stackFrames
            //     };
            //     DebugSystem.Log(logData);
            // }
#endif
        }
        
        public static void Debug(object content)
        {
#if !NOT_PFC_LOG_DEBUG
            Print(LogLevel.Debug, Color.white, null,content.ToString());
#endif
        }
        public static void Debug(object content,Color color)
        {
#if !NOT_PFC_LOG_DEBUG
            Print(LogLevel.Debug, color, null, content.ToString());
#endif
        }
        
        public static void Debug(string channel, object content)
        {
#if !NOT_PFC_LOG_DEBUG
            Print(LogLevel.Debug, Color.white,channel, content);
#endif
        }
        
        public static void Debug(object channel, params object[] content)
        {
#if !NOT_PFC_LOG_DEBUG
            string contentStr = string.Join(' ', content);
            Print(LogLevel.Debug, Color.white,channel.ToString(), contentStr);
#endif
        }
        public static void Debug(string channel, object content,Color color)
        {
#if !NOT_PFC_LOG_DEBUG
            Print(LogLevel.Debug, color, channel, content);
#endif
        }

        public static void Info(object content)
        {
#if !NOT_PFC_LOG_INFO
            Print(LogLevel.Info, Color.white, null,content.ToString());
#endif
        }
        public static void Info(object content,Color color)
        {
#if !NOT_PFC_LOG_INFO
            Print(LogLevel.Info, color, null, content.ToString());
#endif
        }
        
        public static void Info(string channel, object content)
        {
#if !NOT_PFC_LOG_INFO
            Print(LogLevel.Info, Color.white,channel, content);
#endif
        }
        
        public static void Info(object channel, params object[] content)
        {
#if !NOT_PFC_LOG_INFO
            string contentStr = string.Join(' ', content);
            Print(LogLevel.Info, Color.white, channel.ToString(), contentStr);
#endif
        }
        
        public static void Info(string channel, object content,Color color)
        {
#if !NOT_PFC_LOG_INFO
            Print(LogLevel.Info, color, channel, content);
#endif
        }
        
        public static void Warning(object content)
        {
#if !NOT_PFC_LOG_WARNING
            Print(LogLevel.Warning, Color.white, null,content.ToString());
#endif
        }
        
        public static void Warning(object content,Color color)
        {
#if !NOT_PFC_LOG_WARNING
            Print(LogLevel.Warning, Color.white, null,content.ToString());
#endif
        }
        
        public static void Warning(string channel, object content)
        {
#if !NOT_PFC_LOG_WARNING
            Print(LogLevel.Warning, Color.white, null, content);
#endif
        }

        public static void Warning(object channel, params object[] content)
        {
#if !NOT_PFC_LOG_WARNING
            string contentStr = string.Join(' ', content);
            Print(LogLevel.Warning, Color.white, channel.ToString(), contentStr);
#endif
        }

        public static void Warning(string channel, object content,Color color)
        {
#if !NOT_PFC_LOG_WARNING
            Print(LogLevel.Warning, color, channel, content);
#endif
        }
        
        public static void Error(object content)
        {
#if !NOT_PFC_LOG_ERROR
            Print(LogLevel.Error, Color.white, null,content.ToString());
#endif
        }
        
        public static void Error(object content,Color color)
        {
#if !NOT_PFC_LOG_ERROR
            Print(LogLevel.Error, color, null,content.ToString()); 
#endif
        }
        public static void Error(string channel, object content)
        {
#if !NOT_PFC_LOG_ERROR
            Print(LogLevel.Error,Color.white, null,content);
#endif
        }
        
        public static void Error(object channel, params object[] content)
        {
#if !NOT_PFC_LOG_ERROR
            string contentStr = string.Join(' ', content);
            Print(LogLevel.Error, Color.white, channel.ToString(), contentStr);
#endif
        }
        
        public static void Error(string channel, object content,Color color)
        {
#if !NOT_PFC_LOG_ERROR
            Print(LogLevel.Error, color, channel, content);
#endif
        }
    }
}