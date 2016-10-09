using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Neitri
{
	public interface ILogEnd
	{
		void Log(LogEntry logEntry);
	}

	public static class LogEndExtensions
	{
		public static void Trace<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Trace,
				value,
				caller
			));
		}

		public static void Debug<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Debug,
				value,
				caller
			));
		}

		public static void Info<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Info,
				value,
				caller
			));
		}

		public static void Warn<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Warn,
				value,
				caller
			));
		}

		public static void Error<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Error,
				value,
				caller
			));
		}

		public static void Fatal<T>(this ILogEnd log, T value, [CallerMemberName] string caller = null)
		{
			log.Log(new LogEntry(
				LogEntry.LogType.Fatal,
				value,
				caller
			));
		}

		public static void FatalException(this ILogEnd log, Exception e)
		{
			log.Fatal(e);
			var ae = e as AggregateException;
			if (ae != null) foreach (var _e in ae.InnerExceptions) log.FatalException(_e);
		}

		public static LogScope Scope<T>(this ILogEnd log, T value)
		{
			return new LogScope(log, value.ToString());
		}

		public static LogScope ScopeStart<T>(this ILogEnd log, T value)
		{
			var scope = Scope<T>(log, value);
			scope.Start();
			return scope;
		}

		public static LogScope Profile<T>(this ILogEnd log, T value)
		{
			return new LogProfile(log, value.ToString());
		}

		public static LogScope ProfileStart<T>(this ILogEnd log, T value)
		{
			var scope = Profile<T>(log, value);
			scope.Start();
			return scope;
		}
	}

	public class LogScope : ILogEnd, IDisposable
	{
		protected ILogEnd parent;
		protected string scopeName;
		protected bool started;

		public LogScope(ILogEnd parent, string scopeName)
		{
			this.parent = parent;
			this.scopeName = scopeName;
		}

		public void Log(LogEntry logEntry)
		{
			parent.Log(new LogEntry(
				logEntry.Type,
				scopeName + " - " + logEntry.Message
			));
		}

		public virtual void Start()
		{
			if (started) return;
			this.Trace("start");
			started = true;
		}

		public void Dispose()
		{
			End();
		}

		public virtual void End(string differentEndMessage = null)
		{
			if (!started) return;
			if (differentEndMessage.IsNullOrWhiteSpace()) this.Trace("end");
			else this.Trace("end - " + differentEndMessage);
			started = false;
		}
	}

	public class LogProfile : LogScope
	{
		string name;
		Stopwatch time;
		ILogEnd log;

		public LogProfile(ILogEnd parent, string scopeName) : base(parent, scopeName)
		{
		}

		public override void Start()
		{
			if (time == null) time = new Stopwatch();
			time.Start();
			base.Start();
		}

		public override void End(string differentEndMessage = null)
		{
			time.Stop();
			base.End("took " + time.ElapsedMilliseconds + " ms");
		}
	}

	public class LogEntry
	{
		public enum LogType
		{
			Debug,
			Warn,
			Error,
			Fatal,
			Trace,
			Info,
		}

		public LogEntry(LogType type, object message, string caller = null)
		{
			this.Type = type;
			this.message = message.ToString();
			if (!caller.IsNullOrWhiteSpace()) message += " [" + caller + "]";
		}

		public string OneLetterType => Type.ToString().Substring(0, 1);
		public LogType Type { get; }
		string message;

		public string Message
		{
			get
			{
				var indent = "";
				for (short i = 0; i < IndentLevel; i++) indent += "\t";
				return indent + message;
			}
		}

		public short IndentLevel { get; set; }
	}
}