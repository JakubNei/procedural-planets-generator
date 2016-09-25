using System;

namespace Neitri
{
	internal static class ActionExtensions
	{
		// http://stackoverflow.com/questions/231525/raising-c-sharp-events-with-an-extension-method-is-it-bad
		/// <summary>
		/// Shoetcut extension method to invoke Action, so we don't have to check for null everywhere, but can only call a single function;
		/// </summary>
		/// <param name="handler"></param>
		static public void Raise(this Action handler)
		{
			if (handler != null) handler();
		}

		static public void Raise<T1>(this Action<T1> handler, T1 a)
		{
			if (handler != null) handler(a);
		}

		static public void Raise<T1, T2>(this Action<T1, T2> handler, T1 a, T2 b)
		{
			if (handler != null) handler(a, b);
		}

		static public void Raise<T1, T2, T3>(this Action<T1, T2, T3> handler, T1 a, T2 b, T3 c)
		{
			if (handler != null) handler(a, b, c);
		}

		static public void Raise<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> handler, T1 a, T2 b, T3 c, T4 d)
		{
			if (handler != null) handler(a, b, c, d);
		}
	}
}