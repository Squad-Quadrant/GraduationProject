namespace Core.Log
{
	public interface ILog
	{
		void Log(string msg);
		void LogWarning(string msg);
		void LogError(string msg);
	}
}
