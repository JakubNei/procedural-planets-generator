using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neitri.Logging
{
	public class LogAgregator : ILogging
	{
		List<ILogging> loggers = new List<ILogging>();

		public void AddLogger(ILogging log)
		{
			loggers.Add(log);
		}
		public void Error<T>(T value)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Error(value);
		}

		public void Fatal<T>(T value)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Fatal(value);
		}

		public void Info<T>(T value)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Info(value);
		}

		public void Trace<T>(T value)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Trace(value);
		}

		public void Warn<T>(T value)
		{
			for (int i = 0; i < loggers.Count; i++) loggers[i].Warn(value);
		}
	}
}
