using System;
using System.Collections;
using System.Collections.Generic;

namespace MyEngine
{
	public static class ILisExtensions
	{

		public static void AddRange(this IList me, IEnumerable enumerable)
		{
			if (me == null) throw new NullReferenceException("me");
			if (enumerable == null) throw new NullReferenceException("enumerable");
			foreach (object e in enumerable)
			{
				me.Add(e);
			}
		}

		public static void AddRange(this IList me, IList other)
		{
			if (me == null) throw new NullReferenceException("me");
			if (other == null) throw new NullReferenceException("other");
			foreach (object e in other)
			{
				me.Add(e);
			}
		}

		public static void AddRange<T>(this IList<T> me, IEnumerable<T> enumerable)
		{
			if (me == null) throw new NullReferenceException("me");
			if (enumerable == null) throw new NullReferenceException("enumerable");
			foreach (var e in enumerable)
			{
				me.Add(e);
			}
		}

		public static void AddRange<T>(this IList<T> me, IList<T> other)
		{
			if (me == null) throw new NullReferenceException("me");
			if (other == null) throw new NullReferenceException("other");
			foreach (var e in other)
			{
				me.Add(e);
			}
		}
	}
}