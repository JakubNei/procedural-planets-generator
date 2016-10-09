using System;
using System.Collections.Generic;

namespace Neitri.Logging
{
	public class LogAgregator : ILogEnd
	{
		List<ILogEnd> loggers = new List<ILogEnd>();

		public void AddLogger(ILogEnd logEnd)
		{
			loggers.Add(logEnd);
		}

		public void Log(LogEntry logEntry)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Log(logEntry);
		}
	}
}