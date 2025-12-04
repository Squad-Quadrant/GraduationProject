using System;
using Core.Log;
using Data.Config;
using UnityEngine;

namespace Presentation.Logger
{
	public class UnityLoggerFactory : ILoggerFactory
	{
		private readonly LogSettings _settings;

		public UnityLoggerFactory(LogSettings settings)
		{
			_settings = settings;
			if (_settings) return;

			Debug.LogWarning(
				"[UnityLoggerFactory] LogSettings is null. Creating default settings. " +
				"Consider assigning LogSettings in Bootstrapper."
			);
			_settings = ScriptableObject.CreateInstance<LogSettings>();
		}

		public Core.Log.ILogger GetLogger(Type type) => new UnityLogger(type, _settings);
	}
}
