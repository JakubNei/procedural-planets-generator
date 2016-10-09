using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Neitri.Logging
{
	public class LogConsole : ILogEnd
	{
		ConcurrentQueue<Entry> entries = new ConcurrentQueue<Entry>();
		Thread thread;
		ManualResetEvent doLog = new ManualResetEvent(false);

		public LogConsole()
		{
			thread = new Thread(() =>
			{
				while (true)
				{
					doLog.WaitOne();

					Entry entry;
					while (entries.TryDequeue(out entry))
					{
						Console.ForegroundColor = entry.color;
						Console.WriteLine(entry.message);
					}

					doLog.Reset();
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		class Entry
		{
			public ConsoleColor color;
			public string message;
		}

		public void Log(LogEntry logEntry)
		{
			var color = ConsoleColor.Gray;
			switch (logEntry.Type)
			{
				default:
				case LogEntry.LogType.Info:
					color = ConsoleColor.Gray;
					break;

				case LogEntry.LogType.Trace:
				case LogEntry.LogType.Debug:
					color = ConsoleColor.Cyan;
					break;

				case LogEntry.LogType.Warn:
					color = ConsoleColor.Yellow;
					break;

				case LogEntry.LogType.Error:
					color = ConsoleColor.Red;
					break;

				case LogEntry.LogType.Fatal:
					color = ConsoleColor.Magenta;
					break;
			}

			entries.Enqueue(new Entry()
			{
				color = color,
				message = string.Format("[{0}][{1}] {2}",
					DateTime.Now.ToString("HH:mm:ss.fff"),
					logEntry.Type.ToString().Substring(0, 1),
					logEntry.Message
				)
			});

			doLog.Set();
		}
	}
}