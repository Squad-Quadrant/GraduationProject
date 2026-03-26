using System;
using System.Collections.Generic;

namespace Core.Log
{
	public static class LogExtensions
	{
		private static ILoggerFactory _factory;
		private static readonly Dictionary<Type, ILogger> LoggerCache = new();
		private static readonly object Lock = new();
        private static ILogger _gameLogger;

		public static void Initialize(ILoggerFactory factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));

			lock (Lock) LoggerCache.Clear();
		}
        
        public static void RegisterGameLogger(ILogger logger)
        {
            _gameLogger = logger;
        }

		public static void LogDebug(this object obj, string message, bool forPlayer = false, bool format = true)
        {
            if (forPlayer && _gameLogger != null)
            {
                _gameLogger.Debug(message, format);
            }
            GetLogger(obj)?.Debug(message, format);
        }

		public static void Log(this object obj, string message, bool forPlayer = false, bool format = true)
        {
            if (forPlayer && _gameLogger != null)
            {
                _gameLogger.Info(message, format);
            }
            GetLogger(obj)?.Info(message, format);
        }

		public static void LogWarning(this object obj, string message, bool forPlayer = false, bool format = true)
        {
            if (forPlayer && _gameLogger != null)
            {
                _gameLogger.Warning(message, format);
            }
            GetLogger(obj)?.Warning(message, format);
        }

		public static void LogError(this object obj, string message, bool forPlayer = false, bool format = true)
        {
            if (forPlayer && _gameLogger != null)
            {
                _gameLogger.Error(message, format);
            }
            GetLogger(obj)?.Error(message, format);
        }

		private static ILogger GetLogger(object obj)
		{
            // 如果是打包后的版本，直接返回一个空的logger
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            	return null;
#endif
            
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
