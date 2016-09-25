using System;

namespace Neitri.Logging
{
	public class LogConsole : ILogging
	{
		void Log<T>(string level, T value)
		{
			Console.WriteLine(string.Format("[{0}][{1}] {2}", DateTime.Now.ToString("HH:mm:ss.fff"), level, value));
		}

		public void Error<T>(T value)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.BackgroundColor = ConsoleColor.Black;
			Log("E", value);
		}

		public void Fatal<T>(T value)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.BackgroundColor = ConsoleColor.Black;
			Log("F", value);
		}

		public void Info<T>(T value)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			Log("I", value);
		}

		public void Trace<T>(T value)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.BackgroundColor = ConsoleColor.Black;
			Log("T", value);
		}

		public void Warn<T>(T value)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Black;
			Log("W", value);
		}
	}
}