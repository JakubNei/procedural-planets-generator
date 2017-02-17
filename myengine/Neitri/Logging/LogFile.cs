using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Neitri.Logging
{
	public class LogFile : ILog
	{
		StreamWriter streamWriter;
		ConcurrentQueue<string> entries = new ConcurrentQueue<string>();
		Thread thread;
		ManualResetEvent doLog = new ManualResetEvent(false);

		public LogFile(StreamWriter sw)
		{
			this.streamWriter = sw;
			thread = new Thread(() =>
			{
				while (true)
				{
					doLog.WaitOne();

					string msg;
					while (entries.TryDequeue(out msg))
						streamWriter.WriteLine(msg);

					doLog.Reset();
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public void Log(LogEntry logEntry)
		{
			entries.Enqueue(
				string.Format("[{0}][{1}] {2}",
						DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff"),
						logEntry.Type.ToString().Substring(0, 1),
						logEntry.Message
				)
			);
			doLog.Set();
		}
	}
}