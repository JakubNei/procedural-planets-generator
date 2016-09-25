using System;
using System.IO;

namespace Neitri.Logging
{
	public class LogFile : ILogging
	{
		StreamWriter streamWriter;

		public LogFile(StreamWriter sw)
		{
			this.streamWriter = sw;
		}

		void Log<T>(string level, T value)
		{
			streamWriter.WriteLine(string.Format("[{0}][{1}] {2}", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"), level, value));
		}

		public void Error<T>(T value)
		{
			Log("E", value);
		}

		public void Fatal<T>(T value)
		{
			Log("F", value);
		}

		public void Info<T>(T value)
		{
			Log("I", value);
		}

		public void Trace<T>(T value)
		{
			Log("T", value);
		}

		public void Warn<T>(T value)
		{
			Log("W", value);
		}
	}
}