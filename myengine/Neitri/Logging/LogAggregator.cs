using System;
using System.Collections.Generic;

namespace Neitri.Logging
{
	public class LogAgregator : ILog
	{
		List<ILog> loggers = new List<ILog>();

		public void AddLogger(ILog logEnd)
		{
			loggers.Add(logEnd);
		}

		public void Log(LogEntry logEntry)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Log(logEntry);
		}
	}
}