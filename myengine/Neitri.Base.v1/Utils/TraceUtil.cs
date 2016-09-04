using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System;
using Neitri.DependencyInjection;

namespace Neitri
{




	public static class TraceUtil
	{

		public static string GetMethodName(int skipFrames)
		{
			skipFrames++;
			var frame = new StackFrame(skipFrames);
			var m = frame.GetMethod();
			return m.DeclaringType.FullName + "." + m.Name +
				   "(" +
				   string.Join(", ",
					   m.GetParameters()
						   .Select(p => p.ParameterType.Name + " " + p.Name)
						   .ToArray()
					   ) +
				   ")";
		}

		public static Type GetType(int skipFrames)
		{
			skipFrames++;
			var frame = new StackFrame(skipFrames);
			return frame.GetMethod().DeclaringType;
		}

		/// <summary>
		/// Returns the caller method name and class along with params type, name and <paramref name="paramsValue"/>.
		/// </summary>
		/// <param name="paramsValue"></param>
		/// <returns></returns>
		public static string ThisMethod(params object[] paramsValue)
		{
			return ThisMethod(1, paramsValue);
		}

		/// <summary>
		/// Returns the caller method name and class along with params type, name and <paramref name="paramsValue"/>.
		/// </summary>
		/// <param name="paramsValue"></param>
		/// <returns></returns>
		public static string ThisMethod(int skipFrames, params object[] paramsValue)
		{
			skipFrames += 1;
			var frame = new StackFrame(skipFrames);
			var m = frame.GetMethod();

			var s = new StringBuilder();
			s.Append(m.DeclaringType.FullName);
			if (m.IsStatic) s.Append(":");
			else s.Append(".");
			s.Append(m.Name + "(");

			var ps = m.GetParameters();
			for (int i = 0; i < ps.Length; i++)
			{
				if (i > 0) s.Append(", ");
				var paramValue = "";
				if (paramsValue != null && paramsValue.Length > i) paramValue = " = '" + FormatUtils.BetterToString(paramsValue[i]) + "'";
				s.Append(ps[i].ParameterType.Name + " " + ps[i].Name + paramValue);
			}
			s.Append(")");

			return s.ToString();
		}

		public static string NiceStackTrace()
		{
			var st = new StackTrace(1, true);
			return st.ToString();
		}

	}
}