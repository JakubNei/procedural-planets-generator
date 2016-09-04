using UnityEngine;
using System.Diagnostics;
using DD = System.Diagnostics.Debug;
using System;

namespace Neitri
{
	public class UnityLogger : ILogging
	{
		string format = "{0}";
		public UnityLogger()
		{
			format = "[" + TraceUtil.GetType(1).Name + "] {0}";
		}
		public UnityLogger(string format)
		{
			this.format = format;
		}
		[DebuggerNonUserCode]
		string Format<T>(T Value)
		{
			return string.Format(format, Value);
		}
		[DebuggerNonUserCode]
		public void Error<T>(T value)
		{
			Debug.LogError(Format(value));
		}
		[DebuggerNonUserCode]
		public void Fatal<T>(T value)
		{
			if (value is Exception) Debug.LogException(value as Exception);
			else Debug.LogError(Format(value));
		}
		[DebuggerNonUserCode]
		public void Info<T>(T value)
		{
			Debug.Log(Format(value));
		}
		[DebuggerNonUserCode]
		public void Trace<T>(T value)
		{
			Debug.Log(Format(value));
		}
		[DebuggerNonUserCode]
		public void Warn<T>(T value)
		{
			Debug.LogWarning(Format(value));
		}
	}
}
