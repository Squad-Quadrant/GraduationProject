using System;

namespace Core.Log
{
	public interface ILoggerFactory
	{
		ILogger GetLogger(Type type);
	}
}
