using System;
using Core.Log;
using Data.Config;
using UnityEngine;
using Color = UnityEngine.Color;
using ILogger = Core.Log.ILogger;

namespace Presentation.Logger
{
	public class UnityLogger : ILogger
	{
		private readonly Type _type;
		private readonly LogSettings _settings;

		public UnityLogger(Type type, LogSettings settings)
		{
			_type = type ?? throw new ArgumentNullException(nameof(type));
			_settings = settings;
		}

		public void Debug(string message, bool format = true)
		{
			if (_settings && _settings.IsEnabled(_type, LogLevel.Debug))
				UnityEngine.Debug.Log(format ? FormatMessage(message) : message);
		}

		public void Info(string message, bool format = true)
		{
			if (_settings && _settings.IsEnabled(_type, LogLevel.Info))
				UnityEngine.Debug.Log(format ? FormatMessage(message) : message);
		}

		public void Warning(string message, bool format = true)
		{
			if (_settings && _settings.IsEnabled(_type, LogLevel.Warning))
				UnityEngine.Debug.LogWarning(format ? FormatMessage(message) : message);
		}

		public void Error(string message, bool format = true)
		{
			if (_settings && _settings.IsEnabled(_type, LogLevel.Error))
				UnityEngine.Debug.LogError(format ? FormatMessage(message) : message);
		}

		// private string FormatMessage(string message) => $"[{_type.Name}] {message}";
        private string FormatMessage(string message, LogLevel level = LogLevel.Debug)
        {
            var color = GetLogLevelColor(level);
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[{_type.Name}]</color> {message}";
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
