using System.Diagnostics;
using System.Linq;

namespace MyEngine
{
    public static class DiagnosticsUtils
    {
        public static string GetMethodName(int skipFrames = 1)
        {
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
    }
}