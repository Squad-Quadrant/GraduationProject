using System;
using System.Collections.Generic;

namespace Core.Log
{
	public static class LogExtensions
	{
		private static ILoggerFactory _factory;
		private static readonly Dictionary<Type, ILogger> LoggerCache = new();
		private static readonly object Lock = new();

		public static void Initialize(ILoggerFactory factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));

			lock (Lock) LoggerCache.Clear();
		}

		public static void LogDebug(this object obj, string message, bool format = true) => GetLogger(obj)?.Debug(message, format);

		public static void Log(this object obj, string message, bool format = true) => GetLogger(obj)?.Info(message, format);

		public static void LogWarning(this object obj, string message, bool format = true) => GetLogger(obj)?.Warning(message, format);

		public static void LogError(this object obj, string message, bool format = true) => GetLogger(obj)?.Error(message, format);

		private static ILogger GetLogger(object obj)
		{
			if (_factory == null || obj == null)
				return null;

			var type = obj.GetType();

			lock (Lock)
			{
				if (LoggerCache.TryGetValue(type, out var logger))
					return logger;

				logger = _factory.GetLogger(type);
				LoggerCache[type] = logger;
				return logger;
			}
		}
	}
}
