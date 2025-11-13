using Core.Log;
using UnityEngine;

namespace Presentation.Services
{
	public class UnityLog : ILog
	{
		public void Log(string msg) => Debug.Log(msg);

		public void LogWarning(string msg) => Debug.LogWarning(msg);

		public void LogError(string msg) => Debug.LogError(msg);
	}
}
