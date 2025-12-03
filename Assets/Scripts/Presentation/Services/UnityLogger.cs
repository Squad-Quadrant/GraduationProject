using System;
using Core.Log;

namespace Presentation.Services
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

		private string FormatMessage(string message) => $"[{_type.Name}] {message}";
	}
}
