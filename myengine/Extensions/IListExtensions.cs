using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace MyEngine
{
	public static class ILisExtensions
	{
        public static void Resize<T>(this List<T> list, int newCount, T valueToAdd)
        {
            int cur = list.Count;
            if (newCount < cur)
                list.RemoveRange(newCount, cur - newCount);
            else if (newCount > cur)
            {
                if (newCount > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                    list.Capacity = newCount;
                list.AddRange(Enumerable.Repeat(valueToAdd, newCount - cur));
            }
        }
        public static void Resize<T>(this List<T> list, int newCount) where T : new()
        {
            Resize(list, newCount, new T());
        }

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