using Neitri;
using Neitri.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Neitri
{
	public interface ILog
	{
		void Log(LogEntry logEntry);
	}


	public class LogScope : ILog, IDisposable
	{
		protected ILog parent;
		protected string scopeName;
		protected bool started;

		public LogScope(ILog parent, string scopeName)
		{
			this.parent = parent;
			this.scopeName = scopeName;
		}

		public void Log(LogEntry logEntry)
		{
			parent.Log(new LogEntry(
				logEntry.Type,
				scopeName + " - " + logEntry.Message,
				logEntry.Caller
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
			if (differentEndMessage.IsNullOrEmptyOrWhiteSpace()) this.Trace("end");
			else this.Trace("end - " + differentEndMessage);
			started = false;
		}
	}

	public class LogProfile : LogScope
	{
		Stopwatch time;

		public LogProfile(ILog parent, string scopeName) : base(parent, scopeName)
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
			this.Caller = caller;
			if (!caller.IsNullOrEmptyOrWhiteSpace()) message += " [" + caller + "]";
		}

		public string Caller { get; }
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

public static class ILogExtensions
{
#if DEBUG
	const bool debug = true;
#else
	const bool debug = false;
#endif

	static ILog logNothing = new LogNothing();

	public static ILog IfDebug(this ILog log)
	{
		if (debug) return log;
		return logNothing;
	}


	public static ILog IfNotDebug(this ILog log)
	{
		if (debug) return logNothing;
		return log;
	}


	public static void Trace<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Trace,
			value,
			caller
		));
	}

	public static void Debug<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Debug,
			value,
			caller
		));
	}

	public static void Info<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Info,
			value,
			caller
		));
	}

	public static void Warn<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Warn,
			value,
			caller
		));
	}

	public static void Error<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Error,
			value,
			caller
		));
	}

	public static void Fatal<T>(this ILog log, T value, [CallerMemberName] string caller = null)
	{
		log.Log(new LogEntry(
			LogEntry.LogType.Fatal,
			value,
			caller
		));
	}

	public static void Exception(this ILog log, Exception e)
	{
		log.Fatal(e);
		var ae = e as AggregateException;
		if (ae != null) foreach (var _e in ae.InnerExceptions) log.Exception(_e);
	}

	public static LogScope Scope<T>(this ILog log, T value)
	{
		return new LogScope(log, value.ToString());
	}

	public static LogScope ScopeStart<T>(this ILog log, T value)
	{
		var scope = Scope<T>(log, value);
		scope.Start();
		return scope;
	}

	public static LogScope Profile<T>(this ILog log, T value)
	{
		return new LogProfile(log, value.ToString());
	}

	public static LogScope ProfileStart<T>(this ILog log, T value)
	{
		var scope = Profile<T>(log, value);
		scope.Start();
		return scope;
	}
}
