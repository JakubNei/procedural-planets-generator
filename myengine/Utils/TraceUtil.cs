using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System;


namespace MyEngine
{
    public static class TraceUtil
    {

        /// <summary>
        /// Returns the caller method name and class along with params type, name and <paramref name="paramsValue"/>.
        /// </summary>
        /// <param name="paramsValue"></param>
        /// <returns></returns>
        public static string ThisMethod(params object[] paramsValue)
        {
            var skipFrames = 1;
            var frame = new StackFrame(skipFrames);
            var m = frame.GetMethod();

            var s = new StringBuilder();
            s.Append(m.DeclaringType.FullName);
            if (m.IsStatic) s.Append(":");
            else s.Append(".");
            s.Append(m.Name + "(");

            var ps = m.GetParameters();
            for (int i = 0; i < ps.Length; i++) {
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